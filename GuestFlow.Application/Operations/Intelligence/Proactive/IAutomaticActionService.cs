// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Intelligence.Proactive
{
    /// <summary>
    /// Service for executing automatic actions recommended by the intelligence layer.
    /// </summary>
    public interface IAutomaticActionService
    {
        /// <summary>
        /// Executes a specific automatic action recommendation.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>True if execution was successful.</returns>
        Task<bool> ExecuteActionAsync(AutomaticAction action);
    }
}
