using UnityEngine;

public class LifeTimer : MonoBehaviour
{
    public float _LifeTime = 2.0f;
    private float _currentTime;

    public void OnEnable()
    {
        _currentTime = _LifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        _currentTime -= Time.deltaTime;
        if(_currentTime <= 0.0f)
        {
            gameObject.SetActive(false);
            Pool.GetPool().ReturnObject(gameObject);
        }
    }

    public float GetLifePercent()
    {
        return 1.0f - (_currentTime / _LifeTime);
    }
}
