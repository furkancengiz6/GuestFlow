using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Graph.Dtos;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    public interface IGraphIntelligenceService
    {
        /// <summary>
        /// Detects and creates hidden connections between guests based on shared attributes.
        /// </summary>
        Task<List<HiddenConnectionDto>> DiscoverHiddenConnectionsAsync();

        /// <summary>
        /// Analyzes friction points and flags guests at risk.
        /// </summary>
        Task<List<FrictionRiskDto>> PredictFrictionRisksAsync();

        /// <summary>
        /// Identifies influential guests based on their network and interactions.
        /// </summary>
        Task<List<InfluenceDto>> MapGuestInfluenceAsync();
    }
}
