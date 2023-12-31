using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureBehaviour : MonoBehaviour
{
    public enum TemperatureState
    {
        Cold,
        Normal,
        Hot
    }
    
    [Serializable]
    public struct TemperatureRange
    {
        public float cold;
        public float hot;
        public float smoothness;
    }
    [Header("Temperature")]
    public float currentTemperature = 20;
    public float stableTemperature = 20;
    public float temperatureRate = 0.1f;
    public TemperatureRange temperatureRange;
    
    [Header("Min&Max Temperature")]
    public float minTemperature = -20;
    public float maxTemperature = 70;
    
    protected TemperatureState _currentTemperatureState = TemperatureState.Normal;
    public TemperatureState currentTemperatureState
    {
        get
        {
            UpdateSate();
            return _currentTemperatureState;
        }
        set
        {
            var oldState = _currentTemperatureState;
            _currentTemperatureState = value;
            if (value == TemperatureState.Cold)
            {
                currentTemperature = temperatureRange.cold;
            }
            else if (value == TemperatureState.Hot)
            {
                currentTemperature = temperatureRange.hot;
            }
            else if (value == TemperatureState.Normal)
            {
                currentTemperature = stableTemperature;
            }
            OnTemperatureStateChange?.Invoke(oldState,_currentTemperatureState);
            OnTemperatureStateChangeCallback(oldState,_currentTemperatureState);
        }
    }
    public delegate void OnTemperatureStateChangeDelegate(TemperatureState old, TemperatureState state);
    public event OnTemperatureStateChangeDelegate OnTemperatureStateChange;
    
    public List<TemperatureArea> temperatureAreas = new List<TemperatureArea>();

    public void ChangeTemperatureProgressively(float temperature)
    {
        currentTemperature += (temperature - currentTemperature) * temperatureRate * Time.deltaTime;
    }

    private void UpdateSate()
    {
        var oldState = _currentTemperatureState;
        if (currentTemperature < temperatureRange.cold- temperatureRange.smoothness)
        {
            if (_currentTemperatureState != TemperatureState.Cold)
            {
                _currentTemperatureState = TemperatureState.Cold;
                OnTemperatureStateChange?.Invoke(oldState,_currentTemperatureState);
                OnTemperatureStateChangeCallback(oldState,_currentTemperatureState);
            }
        }
        else if (currentTemperature > temperatureRange.hot + temperatureRange.smoothness)
        {
            if (_currentTemperatureState != TemperatureState.Hot)
            {
                _currentTemperatureState = TemperatureState.Hot;
                OnTemperatureStateChange?.Invoke(oldState,_currentTemperatureState);
                OnTemperatureStateChangeCallback(oldState,_currentTemperatureState);
            }
        }
        else if (currentTemperature > temperatureRange.cold + temperatureRange.smoothness && currentTemperature < temperatureRange.hot - temperatureRange.smoothness)
        {
            if (_currentTemperatureState != TemperatureState.Normal)
            {
                _currentTemperatureState = TemperatureState.Normal;
                OnTemperatureStateChange?.Invoke(oldState,_currentTemperatureState);
                OnTemperatureStateChangeCallback(oldState,_currentTemperatureState);
            }
        }
    }

    public virtual void Update()
    {
        UpdateSate();
        //Remove temperature areas that are destroyed or disabled
        temperatureAreas.RemoveAll(area => area == null || !area.gameObject.activeSelf);
        if (GameStateManager.Instance.CurrentGameState == GameState.Gameplay)
        {
            if (temperatureAreas.Count > 0)
            {
                float temperature = 0;
                foreach (var area in temperatureAreas)
                {
                    temperature += area.temperature;
                }

                temperature /= temperatureAreas.Count;
                ChangeTemperatureProgressively(temperature);
            }
            else
            {
                ChangeTemperatureProgressively(stableTemperature);
            }
        }
    }
    
    public virtual void OnTemperatureStateChangeCallback(TemperatureState old,TemperatureState state)
    {
        
    }
    
    public static float Range(float min, float max, float value)
    {
        //return precentage of value between min and max
        return (value - min) / (max - min);
    }
    
    public virtual void TakeDamageTemperature(float damage)
    {
        currentTemperature += damage;
    }
}
