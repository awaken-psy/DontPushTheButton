using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProtagonistLight : MonoBehaviour
{
    Light searchLight;
    Material mt_SearchLight;
    Material mt_GravityGun;
    Material mt_MagenicGun;

    #region 探照灯
    Color searchLightColor;
    float searchLightIntensityDefault;
    float _searchLightIntensity;
    /// <summary>
    /// 探照灯强度（百分比）
    /// </summary>
    public float SearchLightIntensity
    {
        get => _searchLightIntensity;
        set
        {
            _searchLightIntensity = Mathf.Clamp(value, 0, 10);
            mt_SearchLight.SetColor("_EmissionColor", searchLightColor * _searchLightIntensity);
            searchLight.intensity = searchLightIntensityDefault * _searchLightIntensity;
        }
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 开关探照灯
    /// </summary>
    /// <param name="isOn"></param>
    public void SwitchSearchLight(bool isOn)
    {
        searchLight.gameObject.SetActive(isOn);
        if (isOn)
            mt_SearchLight.EnableKeyword("_EMISSION");
        else
            mt_SearchLight.DisableKeyword("_EMISSION");
    }
    /// <summary>
    /// 初始化组件
    /// </summary>
    public void Init()
    {
        if (searchLight == null)
        {
            searchLight = transform.GetChild(6).GetComponent<Light>();
            mt_SearchLight = transform.GetChild(0).GetComponent<MeshRenderer>().materials[5];
            mt_GravityGun = transform.GetChild(3).GetComponent<MeshRenderer>().materials[4];
            mt_MagenicGun = transform.GetChild(4).GetComponent<MeshRenderer>().materials[4];

            searchLightColor = mt_SearchLight.GetColor("_EmissionColor");
            searchLightIntensityDefault = searchLight.intensity;
        }
    }

    #region Test
    [ContextMenuItem("设置强度", "SetLightIntensity")]
    public float 探照灯强度 = 1;
    void SetLightIntensity()
    {
        SearchLightIntensity = 探照灯强度;
    }
    [ContextMenu("开探照灯")]
    void OnSearchLight() => SwitchSearchLight(true);
    [ContextMenu("关探照灯")]
    void OffSearchLight() => SwitchSearchLight(false);
    #endregion
}
