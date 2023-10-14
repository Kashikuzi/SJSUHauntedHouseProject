using System;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Scriptable;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class MotionModule
    {
        [Range(0f, 1f)]
        public float Weight = 1f;

        [NonSerialized] protected MotionPreset preset;
        [NonSerialized] protected PlayerComponent component;
        [NonSerialized] protected Transform transform;
        [NonSerialized] protected string state;

        [NonSerialized] protected CharacterController controller;
        [NonSerialized] protected PlayerStateMachine player;
        [NonSerialized] protected LookController look;

        /// <summary>
        /// Runtime motion parameters.
        /// </summary>
        protected Dictionary<string, object> Parameters => preset.RuntimeParameters;

        /// <summary>
        /// Check whether the module is updatable.
        /// </summary>
        protected bool IsUpdatable
        {
            get
            {
                string currentState = player.StateName;
                return state == MotionBlender.Default || state == currentState;
            }
        }

        public virtual void Initialize(MotionSettings motionSettings)
        {
            preset = motionSettings.preset;
            component = motionSettings.component;
            transform = motionSettings.motionTransform;
            state = motionSettings.motionState;

            controller = component.PlayerCollider;
            player = component.PlayerStateMachine;
            look = component.LookController;
            player.ObservableState.Subscribe(OnStateChange).HandleDisposable();
        }

        public abstract string Name { get; }

        public abstract void MotionUpdate(float deltaTime);
        public abstract Vector3 GetPosition(float deltaTime);
        public abstract Quaternion GetRotation(float deltaTime);

        public virtual void Reset() { }

        public virtual void OnStateChange(string state) { }

        protected abstract void SetTargetPosition(Vector3 target);
        protected abstract void SetTargetRotation(Vector3 target);

        protected abstract void SetTargetPosition(Vector3 target, float multiplier = 1f);
        protected abstract void SetTargetRotation(Vector3 target, float multiplier = 1f);
    }
}