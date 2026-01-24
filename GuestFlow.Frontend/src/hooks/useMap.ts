// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useQuery } from '@tanstack/react-query'
import { mapService } from '../services/mapService'
import type { MapView, MapFilter } from '../types/map'

export const useMapView = (filter?: MapFilter) => {
  return useQuery<MapView>({
    queryKey: ['map', 'view', filter],
    queryFn: () => mapService.getMapView(filter),
    refetchInterval: 30000, // Her 30 saniyede bir yenile
  })
}
