using System;
using System.Collections.Generic;
using System.Linq;

public static class EventManager
{
    // Dictionary to store events with no parameters
    private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();
    // Dictionary to store events with parameters
    private static Dictionary<string, Action<object>> eventDictionaryWithParams = new Dictionary<string, Action<object>>();

    /// <summary>
    /// Subscribe to an event with no parameters.
    /// </summary>
    public static void Subscribe(string eventName, Action listener)
    {
        if (!eventDictionary.TryGetValue(eventName, out var existingListener))
        {
            eventDictionary[eventName] = listener;
        }
        else if (existingListener == null || !existingListener.GetInvocationList().Contains(listener))
        {
            eventDictionary[eventName] += listener;
        }
    }

    /// <summary>
    /// Unsubscribe from an event with no parameters.
    /// </summary>
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    /// <summary>
    /// Trigger an event with no parameters.
    /// </summary>
    public static void TriggerEvent(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName]?.Invoke();
        }
    }

    /// <summary>
    /// Subscribe to an event with parameters.
    /// </summary>
    public static void Subscribe(string eventName, Action<object> listener)
    {
        if (!eventDictionaryWithParams.TryGetValue(eventName, out var existingListener))
        {
            eventDictionaryWithParams[eventName] = listener;
        }
        else if (existingListener == null || !existingListener.GetInvocationList().Contains(listener))
        {
            eventDictionaryWithParams[eventName] += listener;
        }
    }

    /// <summary>
    /// Unsubscribe from an event with parameters.
    /// </summary>
    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName] -= listener;
            if (eventDictionaryWithParams[eventName] == null)
            {
                eventDictionaryWithParams.Remove(eventName);
            }
        }
    }

    /// <summary>
    /// Trigger an event with parameters.
    /// </summary>
    public static void TriggerEvent(string eventName, object parameter)
    {
        if (eventDictionaryWithParams.ContainsKey(eventName))
        {
            eventDictionaryWithParams[eventName]?.Invoke(parameter);
        }
    }
}
