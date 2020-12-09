﻿using System;
using System.Text;
using System.Collections.Generic;

namespace Konata.Runtime.Base
{
    public abstract class Component : BaseObject
    {
        public Entity Parent { get; set; }

        protected Component()
            : base() { }

        protected Component(long id)
            : base(id) { }

        /// <summary>
        /// Get self entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetEntity<T>()
            where T : Entity => (T)Parent;

        /// <summary>
        /// Get another components on this entity 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>()
            where T : Component => Parent?.GetComponent<T>();

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
            Root.Instance.RemoveComponent(Id);
        }
    }
}
