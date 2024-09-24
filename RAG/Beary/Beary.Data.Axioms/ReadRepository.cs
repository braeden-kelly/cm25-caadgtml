﻿using Accord.Collections;
using Beary.Data.Entities;
using Beary.Data.Interfaces;
using Beary.Interfaces;
using Beary.ValueTypes;
using Microsoft.Extensions.Configuration;
using Beary.Data.Axioms.Extensions;
using Accord.MachineLearning;

namespace Beary.Data.Axioms;

public class ReadRepository : IReadEmbeddingsSearchDocuments
{
    private readonly IGetEmbeddings _embeddingsClient;
    private readonly string _filePath;

    private IEnumerable<string>? _axioms;
    private IEnumerable<string> Axioms
    {
        get
        {
            _axioms ??= File.ReadAllLines(_filePath);
            return _axioms;
        }
    }

    private IEnumerable<SearchResult>? _embeddedAxioms;
    private IEnumerable<SearchResult> EmbeddedAxioms
    {
        get
        {
            int i = -1;
            _embeddedAxioms ??= this.Axioms.Select<string, SearchResult>(axiom =>
            {
                i++;
                var id = Guid.NewGuid().ToString();
                var embeddingTask = _embeddingsClient.GetEmbedding(axiom, id);
                embeddingTask.Wait();
                var embedding = embeddingTask.Result;

                return new SearchResult()
                {
                    Content = axiom,
                    Embedding = embedding?.Embedding?.Value,
                    Id = $"{id}_0",
                    ArticleId = id,
                    ElementIndex = i,
                    Score = 0.0
                };
            }).ToList();
            return _embeddedAxioms;
        }
    }

    public ReadRepository(IConfiguration config, IGetEmbeddings embeddingsClient)
    {
        _filePath = config["AxiomFilePath"] ?? string.Empty;
        _embeddingsClient = embeddingsClient;
    }

    public Task<IEnumerable<SearchResult>> GetAllEmbeddings() => Task.FromResult(this.EmbeddedAxioms);

    public Task<long> GetDocumentCount() => Task.FromResult(Convert.ToInt64(this.Axioms.Count()));

    public Task<IEnumerable<SearchResult>> GetNearestNeighbors(Vector queryVector, ResultCount numberOfNeighbors)
    {
        KDTree<double> tree = new KDTree<double>(768);
        this.EmbeddedAxioms.ToList().ForEach(a => tree.Add(a.Embedding!.AsDoubleArray(), a.ElementIndex));
        var neighbors = tree.Nearest(queryVector.Value.AsDoubleArray(), numberOfNeighbors.Value);
        var indexes = neighbors.Select(n => Convert.ToInt32(n.Node.Value));
        return Task.FromResult(_embeddedAxioms!.Where(a => indexes.Contains(a.ElementIndex)));
    }
}
