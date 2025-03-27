using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ItemCell : MonoBehaviour
{
    [Header("图片")]
    public RawImage icon;
    public GameObject spinner;

    [Header("文本")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI describe;

    private Coroutine loadingCoroutine;
    private string currentIconPath = "";

    private static Dictionary<string, Texture> iconCache = new Dictionary<string, Texture>();

    public void SetData(ItemData data)
    {
        title.text = data.itemName;
        describe.text = data.description;

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        currentIconPath = data.iconResourcePath;

        // 检查缓存
        if (iconCache.TryGetValue(currentIconPath, out Texture cachedTexture))
        {
            icon.texture = cachedTexture;
            icon.gameObject.SetActive(true);
            if (spinner != null) spinner.SetActive(false);
            return;
        }

        // 没有缓存，则准备加载
        icon.texture = null;
        icon.gameObject.SetActive(false);
        if (spinner != null) spinner.SetActive(true);

        loadingCoroutine = StartCoroutine(LoadIconAsync(currentIconPath));
    }

    IEnumerator LoadIconAsync(string iconPath)
    {
        string url = $"http://8.129.107.108/wp-content/uploads/2025/03/{iconPath}";

        // 可选模拟延迟
        yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (iconPath != currentIconPath) yield break; // 已切换数据，丢弃旧图

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture tex = DownloadHandlerTexture.GetContent(request);
            iconCache[iconPath] = tex;
            icon.texture = tex;
            icon.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"图标加载失败: {url} | {request.error}");
        }

        if (iconPath == currentIconPath && spinner != null)
            spinner.SetActive(false);

        loadingCoroutine = null;
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
