Goal
Create a checker playing bot via REST Web API that accepts a PDN position and returns the best move using Chinook. Host under IIS as a normal .NET API app.
Stack
Windows Server
IIS with .NET 
Chinook integration
Practical Windows path: run KingsRow on Windows and use the Chinook 2–8 piece databases with it. KingsRow has native Windows builds and can probe those DBs.
https://edgilbert.org/Checkers/KingsRow.htm 
https://edgilbert.org/EnglishCheckers/KingsRowEnglish.htm 
Install Chinook binaries and endgame databases locally.
Provide an EngineAdapter that talks to Chinook via CLI or native DLL.
Adapter methods:
setPosition(pdn)
search(limits) -> { bestMove, pv, scoreOrWDL, nodes, depth, tablebaseHit }


Keep a small pool of long lived Chinook worker processes. Do not spawn per request.
API endpoints
POST /v1/move/suggest
 Request
{
  "gameId": "checkers-8x8",
  "state": { "notation": "PDN", "position": "B:W18,19,22,25,27,28,30,32:B1,5,6,7,10,12,14,16" },
  "level": "weak",
  "limits": { "maxDepth": 12, "softTimeMs": 250, "hardTimeMs": 1200 }
}

Response
{
  "engine": "chinook",
  "bestMove": "22-18x11-7",
  "pv": ["22-18","5-9","18x11","7-16","30-26"],
  "scoreOrWDL": 0,
  "depth": 12,
  "nodes": 153201,
  "positionKey": "pdn:B:W18,19,22,25,27,28,30,32:B1,5,6,7,10,12,14,16",
  "info": { "tablebaseHit": false, "timeMs": 281 }
}

POST /v1/move/validate
Input: { position, move }


Output: { legal: true|false }


GET /healthz
Output: { ok: true, workers: 2 }


Strength levels
weak: depth 6 to 8 or movetime 100 ms. No randomness.


medium: depth 10 to 12 or movetime 250 ms.


strong: probe tablebases first. If not in DB, depth 14 to 18 or movetime 500 to 600 ms.


Flow per request
Parse and normalize PDN. Validate squares and piece counts.
If total pieces <= 8, probe Chinook DB and return the perfect move immediately.
Else call Chinook search with limits based on level.
Verify returned move is legal. If not, try next PV move or return 500.
Cache by canonical PDN for 15 minutes (LRU).
Ops and hosting
Run as a normal ASP.NET Core Web API app behind IIS. No Windows Service.
On app start, create 2 Chinook workers and warm them up.
Round robin request routing to workers with async lock per worker.
Enforce softTimeMs inside adapter and hardTimeMs with CancellationToken at the controller.
Log JSON per request: requestId, timeMs, depth, nodes, tablebaseHit.


Config example
appsettings.json
{
  "Engine": {
    "Type": "chinook",
    "Path": "C:\\engines\\chinook\\chinook.exe",
    "Workers": 2,
    "Databases": "D:\\tb\\chinook"
  },
  "Cache": { "Capacity": 20000, "TtlMinutes": 15 },
  "Limits": { "DefaultSoftTimeMs": 300, "DefaultHardTimeMs": 1200 }
}

Acceptance
Health check returns ok on startup.
Tablebase position with <= 8 pieces returns in under 50 ms with tablebaseHit true.
Midgame position level strong returns in under 600 ms with a legal move.
Invalid PDN gets 422. Timeouts return 504.

