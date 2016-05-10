﻿namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Represents mesh index which does nothing. </summary>
    internal sealed class DummyMeshIndex : IMeshIndex
    {
        public static readonly DummyMeshIndex Default = new DummyMeshIndex();
        
        private DummyMeshIndex() { }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public MeshQuery.Result Modify(MeshQuery query)
        {
            return new MeshQuery.Result(query.Vertices);
        }
    }
}