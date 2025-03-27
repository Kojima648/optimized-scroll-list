using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class RareItemCell : MonoBehaviour
{
    [Header("图片")]
    public RawImage icon;
    public GameObject spinner;

    [Header("文本")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI describe;
    public TextMeshProUGUI quality;

    private Coroutine loadingCoroutine;
    private string currentIconPath = "";

    private static Dictionary<string, Texture> iconCache = new Dictionary<string, Texture>();

    public void SetData(ItemData data)
    {
        title.text = data.itemName;
        describe.text = data.description;

        // 设置品质文字与颜色
        if (!string.IsNullOrEmpty(data.quality))
        {
            quality.text = data.quality;
            switch (data.quality)
            {
                case "普通": quality.color = Color.gray; break;
                case "优秀": quality.color = Color.green; break;
                case "稀有": quality.color = Color.cyan; break;
                case "史诗": quality.color = new Color(0.8f, 0.4f, 1f); break;
                default: quality.color = Color.white; break;
            }
        }
        else
        {
            quality.text = "";
            quality.color = Color.white;
        }

        string iconPath = data.iconResourcePath;

        // 如果是相同图标且已加载，不需要再次加载
        if (currentIconPath == iconPath && icon.texture != null)
        {
            return;
        }

        currentIconPath = iconPath;

        // 如果缓存中已有，立即设置，不显示 loading
        if (iconCache.TryGetValue(iconPath, out Texture cached))
        {
            icon.texture = cached;
            icon.gameObject.SetActive(true);
            if (spinner != null) spinner.SetActive(false);
            return;
        }

        // 准备加载新图标
        icon.texture = null;
        icon.gameObject.SetActive(false);
        if (spinner != null) spinner.SetActive(true);

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadIconAsync(iconPath));
    }

    IEnumerator LoadIconAsync(string iconPath)
    {
        string url = $"http://8.129.107.108/wp-content/uploads/2025/03/{iconPath}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture tex = DownloadHandlerTexture.GetContent(request);

                if (iconPath == currentIconPath)
                {
                    icon.texture = tex;
                    iconCache[iconPath] = tex;
                    icon.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning($"图标加载失败: {url}，错误: {request.error}");
            }

            if (iconPath == currentIconPath && spinner != null)
                spinner.SetActive(false);
        }

        loadingCoroutine = null;
    }

    void OnDisable()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        // 注意：这里不再清除 icon.texture，以防止“滑动后立刻隐藏再重新加载”的情况
        icon.gameObject.SetActive(false);
        if (spinner != null) spinner.SetActive(false);
    }
}
