using System;
using System.Threading;

namespace Zyowo
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    public class Singleton<T> where T : new()
    {
        private static T s_singleton = default(T);
        private static object s_objectLock = new object();
        public static T Instance
        {
            get
            {
                if (Singleton<T>.s_singleton == null)
                {
                    object obj;
                    Monitor.Enter(obj = Singleton<T>.s_objectLock);//加锁防止多线程创建单例
                    try
                    {
                        if (Singleton<T>.s_singleton == null)
                        {
                            Singleton<T>.s_singleton = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));//创建单例的实例
                        }
                    }
                    finally
                    {
                        Monitor.Exit(obj);
                    }
                }
                return Singleton<T>.s_singleton;
            }
        }
        protected Singleton()
        {
        }
    }
    /// <summary>
    /// 基于 monobehaviour 的单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T _current;
        private static readonly object _lockObj = new object();
        private static bool _initailized = false;
        /// <summary>
        /// 当前实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!_initailized)
                {

                    //UnityEngine.Debug.Log("实例化 " + typeof(T).ToString());
                    lock (_lockObj)
                    {
                        Type[] components = new Type[] { typeof(T) };
                        _current = new GameObject(typeof(T).ToString(), components).GetComponent<T>();
                        _initailized = true;
                        DontDestroyOnLoad(_current);

                    }

                }

                return _current;
            }
        }
        /// <summary>
        /// 初始化启动工作
        /// </summary>
        public virtual void Work()
        {
            //初始化启动工作

        }
    }
    /// <summary>
    /// 基于Mono跨场景不销毁的单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DontDestroyMonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _current;
        private static readonly object _lockObj = new object();
        private static bool _initailized = false;
        /// <summary>
        /// 当前实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!_initailized)
                {

                    //UnityEngine.Debug.Log("实例化 " + typeof(T).ToString());
                    lock (_lockObj)
                    {
                        Type[] components = new Type[] { typeof(T) };
                        _current = new GameObject(typeof(T).ToString(), components).GetComponent<T>();
                        _initailized = true;
                        DontDestroyOnLoad(_current);

                    }

                }

                return _current;
            }
        }
        public void Work() { }

    }
}
