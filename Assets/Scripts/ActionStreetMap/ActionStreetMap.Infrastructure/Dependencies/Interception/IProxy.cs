﻿using ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors;

namespace ActionStreetMap.Infrastructure.Dependencies.Interception
{
    /// <summary> Represents a behavior of proxy. </summary>
    public interface IProxy
    {
        /// <summary> Returns wrapped instance. </summary>
        object Instance { get; set; }

        /// <summary> Adds new behavior to wrapped instance. </summary>
        void AddBehavior(IBehavior behavior);

        /// <summary> Clear list of behaviors. </summary>
        void ClearBehaviors();
    }
}