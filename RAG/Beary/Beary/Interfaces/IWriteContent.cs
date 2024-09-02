﻿using Beary.Entities;
using Beary.ValueTypes;

namespace Beary.Interfaces;

public interface IWriteContent
{
    Task SaveAsync(Identifier articleId, ArticleContent fullText);
    Task SaveAsync(Identifier articleId, ArticleContent fullText, TokenCount articleTokens, IEnumerable<ContentChunk> chunks);

    // Task SaveAsync(Article article);
}
