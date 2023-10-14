using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class ManagerModule
    {
        public GameManager GameManager { get; internal set; }

        protected Inventory Inventory => GameManager.Inventory;
        protected PlayerPresenceManager PlayerPresence => GameManager.PlayerPresence;

        public abstract string Name { get; }

        /// <summary>
        /// Override this method to define your own behavior at Awake.
        /// </summary>
        public virtual void OnAwake() { }

        /// <summary>
        /// Override this method to define your own behavior at Start.
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Override this method to define your own behavior at Update.
        /// </summary>
        public virtual void OnUpdate() { }
    }
}