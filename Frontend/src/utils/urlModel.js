function pick(value, keys, fallback = "") {
  for (const key of keys) {
    if (value?.[key] !== undefined && value?.[key] !== null) {
      return value[key];
    }
  }
  return fallback;
}

export function normalizeUrlRecord(raw) {
  return {
    id: pick(raw, ["id", "Id"]),
    shortUrlCode: pick(raw, ["shortUrlCode", "ShortUrlCode"]),
    longUrl: pick(raw, ["longUrl", "LongUrl"]),
    shortUrl: pick(raw, ["shortUrl", "ShortUrl"]),
    description: pick(raw, ["description", "Description"]),
    clicks: Number(pick(raw, ["clicks", "Clicks"], 0)),
    createdAt: pick(raw, ["createdAt", "CreatedAt"]),
    userId: pick(raw, ["userId", "UserId"]),
  };
}

export function normalizeUrlList(rawList) {
  if (!Array.isArray(rawList)) return [];
  return rawList.map(normalizeUrlRecord);
}
