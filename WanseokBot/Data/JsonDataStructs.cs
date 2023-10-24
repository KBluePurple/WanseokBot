using Newtonsoft.Json;

namespace WanseokBot.Data;

public class CalenderResponse
{
    [JsonProperty("response")] public Response Response { get; set; } = null!;
}

public class Response
{
    [JsonProperty("header")] public Header Header { get; set; } = null!;

    [JsonProperty("body")] public Body Body { get; set; } = null!;
}

public class Body
{
    [JsonProperty("items")] public Items Items { get; set; } = null!;

    [JsonProperty("numOfRows")] public long NumOfRows { get; set; }

    [JsonProperty("pageNo")] public long PageNo { get; set; }

    [JsonProperty("totalCount")] public long TotalCount { get; set; }
}

public class Items
{
    [JsonProperty("item")] public Item[] Item { get; set; } = null!;
}

public class Item
{
    [JsonProperty("dateKind")] public string DateKind { get; set; } = null!;

    [JsonProperty("dateName")] public string DateName { get; set; } = null!;

    [JsonProperty("isHoliday")] public string IsHoliday { get; set; } = null!;

    [JsonProperty("locdate")] public long Locdate { get; set; }

    [JsonProperty("seq")] public long Seq { get; set; }
}

public class Header
{
    [JsonProperty("resultCode")] public string ResultCode { get; set; } = null!;

    [JsonProperty("resultMsg")] public string ResultMsg { get; set; } = null!;
}