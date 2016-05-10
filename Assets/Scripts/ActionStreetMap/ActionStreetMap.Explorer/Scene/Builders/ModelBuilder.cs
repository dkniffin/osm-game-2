﻿using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Defines model builder logic. </summary>
    public interface IModelBuilder
    {
        /// <summary> Name of model builder. </summary>
        string Name { get; }

        /// <summary> Builds model from area. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="area">Area.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildArea(Tile tile, Rule rule, Area area);

        /// <summary> Builds model from way. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="way">Way.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildWay(Tile tile, Rule rule, Way way);

        /// <summary> Builds model from node. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="node">Node.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildNode(Tile tile, Rule rule, Node node);
    }

    /// <summary> Defines base class for model builders which provides helper logic. </summary>
    internal abstract class ModelBuilder : IModelBuilder
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        #region Properties. These properties are public due to Reflection limitations on some platform

        /// <summary> Gets trace. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Gets behaviour provider. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public CustomizationService CustomizationService { get; set; }

        /// <summary> Gets elevation provider. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public IElevationProvider ElevationProvider { get; set; }

        /// <summary> Game object factory. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        /// <summary> Gets object pool. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        #endregion

        /// <inheritdoc />
        public virtual IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            //Trace.Debug("model.builder","{0}: building area {1}", Name, area.Id);
            return null;
        }

        /// <inheritdoc />
        public virtual IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            //Trace.Debug("model.builder", "{0}: building way {1}", Name, way.Id);
            return null;
        }

        /// <inheritdoc />
        public virtual IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            //Trace.Debug("model.builder", "{0}: building node {1}", Name, node.Id);
            return null;
        }

        /// <summary> Builds game object from meshData </summary>
        protected virtual void BuildObject(IGameObject parent, MeshData meshData, Rule rule, Model model)
        {
            Observable.Start(() =>
            {
                var gameObject = meshData.GameObject.AddComponent(new GameObject());
                var mesh = new Mesh();
                mesh.vertices = meshData.Vertices;
                mesh.triangles = meshData.Triangles;
                mesh.colors = meshData.Colors;
                mesh.RecalculateNormals();

                gameObject.AddComponent<MeshFilter>().mesh = mesh;
                gameObject.AddComponent<MeshCollider>();
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = CustomizationService
                    .GetMaterial(meshData.MaterialKey);

                // attach behaviours
                gameObject.AddComponent<MeshIndexBehaviour>().Index = meshData.Index;
                var behaviourTypes = rule.GetModelBehaviours(CustomizationService);
                foreach (var behaviourType in behaviourTypes)
                {
                    var behaviour = gameObject.AddComponent(behaviourType) as IModelBehaviour;
                    if (behaviour != null)
                        behaviour.Apply(meshData.GameObject, model);
                }

                gameObject.isStatic = true;
                gameObject.transform.parent = parent.GetComponent<GameObject>().transform;
            }, Scheduler.MainThread);
        }

        /// <summary> Returns name of game object. </summary>
        /// <param name="model">Model.</param>
        /// <returns>Name of game object.</returns>
        protected string GetName(Model model)
        {
            return String.Format("{0} {1}", Name, model);
        }
    }
}
