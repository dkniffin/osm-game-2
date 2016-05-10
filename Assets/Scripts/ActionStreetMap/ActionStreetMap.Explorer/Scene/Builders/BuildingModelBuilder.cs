﻿using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Facades;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build buildings. </summary>
    internal class BuildingModelBuilder : ModelBuilder
    {
        private readonly IEnumerable<IFacadeBuilder> _facadeBuilders;
        private readonly IEnumerable<IRoofBuilder> _roofBuilders;

        /// <inheritdoc />
        public override string Name { get { return "building"; } }

        /// <summary> Creates instance of <see cref="BuildingModelBuilder"/>. </summary>
        [Dependency]
        public BuildingModelBuilder(IEnumerable<IFacadeBuilder> facadeBuilders, 
                                    IEnumerable<IRoofBuilder> roofBuilders)
        {
            _facadeBuilders = facadeBuilders.ToArray();
            _roofBuilders = roofBuilders.ToArray();
        }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);
            return BuildBuilding(tile, rule, area, area.Points);
        }

        private IGameObject BuildBuilding(Tile tile, Rule rule, Model model, List<GeoCoordinate> footPrint)
        {
            if (tile.Registry.Contains(model.Id))
                return null;

            var points = ObjectPool.NewList<Vector2d>();
            PointUtils.GetClockwisePolygonPoints(tile.RelativeNullPoint, footPrint, points);

            var minHeight = BuildingRuleExtensions.GetMinHeight(rule);

            // TODO invent better algorithm
            var elevation = ElevationProvider.GetElevation(points[0]);

            var gameObject = BuildGameObject(tile, rule, model, points, elevation, minHeight);

            ObjectPool.StoreList(points);

            return gameObject;
        }

        private IGameObject BuildGameObject(Tile tile, Rule rule, Model model, List<Vector2d> points,
            float elevation, float minHeight)
        {
            tile.Registry.RegisterGlobal(model.Id);

            var gameObjectWrapper = GameObjectFactory
                .CreateNew(GetName(model), tile.GameObject);

            var isPart = rule.IsPart();
            var height = rule.GetHeight();

            // NOTE: this is not clear
            //if (isPart)
            height -= minHeight;

            var building = new Building
            {
                Id = model.Id,
                GameObject = gameObjectWrapper,
                IsPart = isPart,
                Height = height,
                Levels = rule.GetLevels(),
                MinHeight = minHeight,
                HasWindows = rule.HasWindows(),
                FacadeType = rule.GetFacadeBuilder(),
                FacadeColor = rule.GetFacadeColor(),
                FacadeMaterial = rule.GetFacadeMaterial(),
                RoofType = rule.GetRoofBuilder(),
                RoofColor = rule.GetRoofColor(),
                RoofMaterial = rule.GetRoofMaterial(),
                RoofHeight = rule.GetRoofHeight(),
                FloorFrontColor = rule.GetFloorFrontColor(),
                FloorBackColor = rule.GetFloorBackColor(),
                Elevation = elevation,
                Footprint = points,
            };

            // facade
            var facadeBuilder = _facadeBuilders.Single(f => f.Name == building.FacadeType);
            var facadeMeshDataList = facadeBuilder.Build(building);
            foreach (var facadeMeshData in facadeMeshDataList)
            {
                facadeMeshData.GameObject = GameObjectFactory.CreateNew("wall");
                facadeMeshData.MaterialKey = building.FacadeMaterial;
                BuildObject(gameObjectWrapper, facadeMeshData, rule, model);
            }

            // roof
            var roofBuilder = _roofBuilders.Single(f => f.Name == building.RoofType);
            var roofMeshDataList = roofBuilder.Build(building);
            foreach (var roofMeshData in roofMeshDataList)
            {
                roofMeshData.GameObject = GameObjectFactory.CreateNew("floor");
                roofMeshData.MaterialKey = building.RoofMaterial;
                BuildObject(gameObjectWrapper, roofMeshData, rule, model);
            }
            return gameObjectWrapper;
        }
    }
}