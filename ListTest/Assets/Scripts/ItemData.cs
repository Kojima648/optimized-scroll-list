
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public string description;
    public string iconResourcePath;
    public string quality;
}


[System.Serializable]
public class ResponseWrapper
{
    public List<ItemData> data;
    public Pagination pagination;
}

[System.Serializable]
public class Pagination
{
    public int current_page;
    public int page_size;
    public int total_items;
    public int total_pages;
}