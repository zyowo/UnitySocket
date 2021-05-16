/**     
 *      Date:    2020/03/30
 *      Author:  yiwen.zhong
 *      
 *      事件帮助类，用于实现观察者模式的事件监听/触发
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zyowo
{
	// 在这里定义事件名，方便收集
	public class EType
	{
		public const string CLIENT_CONNECTED = "客户端连接成功";
	}

	public class EventListener : MonoBehaviour
	{
		// 使用唯一的 事件派发器 实例
		public static readonly UEventDispatcher dispatcher = new UEventDispatcher();

		// 添加事件
		public static void AddEvent(string eType, UEventListener.EventListenerDelegate callback)
		{
			dispatcher.addEventListener(eType, callback);
		}

		// 触发事件
		public static void Invoke(string eType, params object[] eParams)
		{
			dispatcher.dispatchEvent(new UEvent(eType, eParams));
		}

		// 移除事件
		public static void Remove(string eType, UEventListener.EventListenerDelegate callback)
		{
			dispatcher.removeEventListener(eType, callback);
		}
	}

	// 事件监听器
	public class UEventListener
	{
		/// <summary>
		/// 事件类型
		/// </summary>
		public string eventType;

		public UEventListener(string eventType)
		{
			this.eventType = eventType;
		}

		public delegate void EventListenerDelegate(UEvent evt);
		public event EventListenerDelegate OnEvent;

		public void Excute(UEvent evt)
		{
			if (OnEvent != null)
			{
				this.OnEvent(evt);
			}
		}
	}

	// 事件参数
	public class UEvent
	{
		/// <summary>
		/// 事件类别
		/// </summary>
		public string eventType;

		/// <summary>
		/// 参数
		/// </summary>
		public object[] eventParams;

		/// <summary>
		/// 事件抛出者 -- 暂时不用 --
		/// </summary>
		public object target;

		public UEvent(string eventType, params object[] eParams)
		{
			this.eventType = eventType;
			this.eventParams = eParams;
		}
	}

	// 事件触发器
	public class UEventDispatcher
	{
		protected IList<UEventListener> eventListenerList;

		public UEventDispatcher()
		{
			this.eventListenerList = new List<UEventListener>();
		}

		/// <summary>
		/// 侦听事件
		/// </summary>
		/// <param name="eventType">事件类别</param>
		/// <param name="callback">回调函数</param>
		public void addEventListener(string eventType, UEventListener.EventListenerDelegate callback)
		{
			UEventListener eventListener = this.getListener(eventType);
			if (eventListener == null)
			{
				eventListener = new UEventListener(eventType);
				eventListenerList.Add(eventListener);
			}

			eventListener.OnEvent += callback;
		}

		/// <summary>
		/// 移除事件
		/// </summary>
		/// <param name="eventType">事件类别</param>
		/// <param name="callback">回调函数</param>
		public void removeEventListener(string eventType, UEventListener.EventListenerDelegate callback)
		{
			UEventListener eventListener = this.getListener(eventType);
			if (eventListener != null)
			{
				eventListener.OnEvent -= callback;
			}
		}

		/// <summary>
		/// 是否存在事件
		/// </summary>
		/// <returns><c>true</c>, if listener was hased, <c>false</c> otherwise.</returns>
		/// <param name="eventType">Event type.</param>
		public bool hasListener(string eventType)
		{
			return this.getListenerList(eventType).Count > 0;
		}

		/// <summary>
		/// 发送事件
		/// </summary>
		/// <param name="evt">Evt.</param>
		/// <param name="gameObject">Game object.</param>
		public void dispatchEvent(UEvent evt)
		{
			IList<UEventListener> resultList = this.getListenerList(evt.eventType);

			foreach (UEventListener eventListener in resultList)
			{
				eventListener.Excute(evt);
			}
		}

		/// <summary>
		/// 获取事件列表
		/// </summary>
		/// <returns>The listener list.</returns>
		/// <param name="eventType">Event type.</param>
		private IList<UEventListener> getListenerList(string eventType)
		{
			IList<UEventListener> resultList = new List<UEventListener>();
			foreach (UEventListener eventListener in this.eventListenerList)
			{
				if (eventListener.eventType == eventType) resultList.Add(eventListener);
			}
			return resultList;
		}

		/// <summary>
		/// 获取事件
		/// </summary>
		/// <returns>The listener.</returns>
		/// <param name="eventType">Event type.</param>
		private UEventListener getListener(string eventType)
		{
			foreach (UEventListener eventListener in this.eventListenerList)
			{
				if (eventListener.eventType == eventType) return eventListener;
			}
			return null;
		}
	}
}