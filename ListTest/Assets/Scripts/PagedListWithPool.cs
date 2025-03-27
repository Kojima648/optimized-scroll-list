using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PagedListWithPool : MonoBehaviour
{
    [Header("UI 组件")]
    public ScrollRect scrollRect;
    public RectTransform contentRect;
    public GameObject itemPrefab;
    public GameObject rareItemPrefab;
    public Transform contentParent;

    [Header("功能按钮")]
    public Button loadMoreButton;
    public Button backToTopButton;
    public Button refreshButton;
    public Button switchDataButton;

    [Header("分页设置")]
    public string apiBaseUrl = "http://8.129.107.108/wp-json/custom/v1/ninja-pro-table";
    public int tableId = 12352;
    public int rareTableId = 12353;
    public int pageSize = 30;

    [Header("虚拟滚动设置")]
    public float itemHeight = 150f;
    public int bufferCount = 5;

    private List<ItemData> allData = new List<ItemData>();
    private List<GameObject> itemPool = new List<GameObject>();
    private int poolSize;
    private int currentPage = 0;
    private int totalPages = int.MaxValue;
    private int lastStartIndex = -1;
    private bool isRareMode = false;
    private bool isLoading = false;

    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScrollChanged);

        loadMoreButton?.onClick.AddListener(() =>
        {
            if (!isLoading && currentPage < totalPages)
                LoadPage(currentPage + 1);
        });

        backToTopButton?.onClick.AddListener(() =>
        {
            scrollRect.verticalNormalizedPosition = 1f;
        });

        refreshButton?.onClick.AddListener(() =>
        {
            RefreshCurrentVisibleItems();
        });

        switchDataButton?.onClick.AddListener(() =>
        {
            SwitchDataSource();
        });

        InitItemPool();
        LoadPage(1);
    }

    void InitItemPool()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        itemPool.Clear();

        int visibleCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / itemHeight);
        poolSize = visibleCount + bufferCount * 2;

        GameObject prefabToUse = isRareMode ? rareItemPrefab : itemPrefab;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = isRareMode ? rareItemPrefab : itemPrefab;
            var go = Instantiate(prefab, contentParent);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(0, -i * itemHeight);

            go.SetActive(false);
            itemPool.Add(go);
        }

    }

    void SwitchDataSource()
    {
        isRareMode = !isRareMode;
        tableId = isRareMode ? rareTableId : 12352;
        allData.Clear();
        currentPage = 0;
        totalPages = int.MaxValue;
        lastStartIndex = -1;

        // 关键修复：重置 Content 位置
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0);

        // 修复锚点偏移问题
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.anchoredPosition = new Vector2(0, 0); // 重置 position

        InitItemPool();
        StartCoroutine(DelayedLoadPage());
    }

    IEnumerator DelayedLoadPage()
    {
        yield return null; // 等一帧，确保 InitItemPool 创建完成
        LoadPage(1);
    }



    void LoadPage(int page)
    {
        StartCoroutine(FetchPage(page));
    }

    IEnumerator FetchPage(int page)
    {
        isLoading = true;

        string url = $"{apiBaseUrl}?id={tableId}&page={page}&size={pageSize}";
        Debug.Log("请求数据: " + url);

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var parsed = JsonUtility.FromJson<ResponseWrapper>(request.downloadHandler.text);
            currentPage = parsed.pagination.current_page;
            totalPages = parsed.pagination.total_pages;

            allData.AddRange(parsed.data);
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, allData.Count * itemHeight);

            UpdateVisibleItems();
        }
        else
        {
            Debug.LogError("分页请求失败: " + request.error);
        }

        isLoading = false;
        loadMoreButton?.gameObject.SetActive(currentPage < totalPages);
    }

    void RefreshCurrentVisibleItems()
    {
        if (isLoading) return;

        int startIndex = Mathf.FloorToInt(contentRect.anchoredPosition.y / itemHeight);
        int refreshPage = (startIndex / pageSize) + 1;
        StartCoroutine(RefetchPage(refreshPage));
    }

    IEnumerator RefetchPage(int page)
    {
        isLoading = true;

        string url = $"{apiBaseUrl}?id={tableId}&page={page}&size={pageSize}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var parsed = JsonUtility.FromJson<ResponseWrapper>(request.downloadHandler.text);
            int baseIndex = (parsed.pagination.current_page - 1) * pageSize;

            while (allData.Count < baseIndex + parsed.data.Count)
                allData.Add(new ItemData());

            for (int i = 0; i < parsed.data.Count; i++)
                allData[baseIndex + i] = parsed.data[i];

            UpdateVisibleItems();
        }

        isLoading = false;
    }

    void OnScrollChanged(Vector2 pos)
    {
        UpdateVisibleItems();
    }

    void UpdateVisibleItems()
    {
        if (allData == null || allData.Count == 0)
        {
            //Debug.LogWarning("allData 为空，无法更新可见项！");
            return;
        }

        float scrollY = contentRect.anchoredPosition.y;
        int startIndex = Mathf.FloorToInt(scrollY / itemHeight);
        if (startIndex < 0) startIndex = 0;

        if (startIndex == lastStartIndex) return;
        lastStartIndex = startIndex;

        //Debug.Log($"当前数据总数：{allData.Count}，itemPool.Count: {itemPool.Count}，startIndex: {startIndex}");

        for (int i = 0; i < itemPool.Count; i++)
        {
            int dataIndex = startIndex + i;
            GameObject cellGO = itemPool[i];

            if (dataIndex < 0 || dataIndex >= allData.Count)
            {
                cellGO.SetActive(false);
                continue;
            }

            cellGO.SetActive(true);

            // 设置数据
            if (isRareMode)
            {
                var rare = cellGO.GetComponent<RareItemCell>();
                if (rare != null)
                    rare.SetData(allData[dataIndex]);
                else
                    Debug.LogWarning($"Item[{i}] 缺少 RareItemCell 脚本");
            }
            else
            {
                var normal = cellGO.GetComponent<ItemCell>();
                if (normal != null)
                    normal.SetData(allData[dataIndex]);
                else
                    Debug.LogWarning($"Item[{i}] 缺少 ItemCell 脚本");
            }

            // 定位
            var rt = cellGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);
        }

        // 判断是否接近底部，加载下一页
        int lastVisibleIndex = startIndex + itemPool.Count - 1;
        if (!isLoading && currentPage < totalPages && lastVisibleIndex >= allData.Count - bufferCount)
        {
            Debug.Log("触发加载下一页...");
            LoadPage(currentPage + 1);
        }
    }
}
