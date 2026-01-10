/* Disabled: legacy Redis cache unit tests.
   These tests targeted distributed cache APIs (GetStringAsync/SetStringAsync)
   which do not match the current InMemoryCacheService implementation.
   To re-enable, rewrite tests against ICacheService (GetAsync/SetAsync) or
   provide a proper Redis-backed IDistributedCache mock.
*/

