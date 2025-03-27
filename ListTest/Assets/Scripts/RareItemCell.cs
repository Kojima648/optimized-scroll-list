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

        // 判断是否为已加载的图标
        if (currentIconPath == data.iconResourcePath && icon.texture != null)
        {
            icon.gameObject.SetActive(true);
            if (spinner != null) spinner.SetActive(false);
            return;
        }

        currentIconPath = data.iconResourcePath;

        icon.texture = null;
        icon.gameObject.SetActive(false);
        if (spinner != null) spinner.SetActive(true);

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadIconAsync(currentIconPath));
    }

    IEnumerator LoadIconAsync(string iconPath)
    {
        string url = $"http://8.129.107.108/wp-content/uploads/2025/03/{iconPath}";

        // 模拟加载延迟
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

        // 先查缓存
        if (iconCache.TryGetValue(iconPath, out Texture cached))
        {
            if (iconPath == currentIconPath)
            {
                icon.texture = cached;
                icon.gameObject.SetActive(true);
                if (spinner != null) spinner.SetActive(false);
            }
            yield break;
        }

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
                Debug.LogWarning("图标加载失败: " + url + "，错误：" + request.error);
                // 可设置默认 fallback 图标
            }

            if (iconPath == currentIconPath && spinner != null)
                spinner.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        icon.texture = null;
        icon.gameObject.SetActive(false);
        if (spinner != null) spinner.SetActive(false);
    }
}
