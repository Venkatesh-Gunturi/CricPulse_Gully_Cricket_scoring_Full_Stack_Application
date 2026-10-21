# CricPulse backend refactor — unified User identity

## What changed

- `User.Id` is the canonical identity for accounts, match participation, player statistics, and match organization.
- Cricket details (`DateOfBirth`, `Gender`, `State`, `PinCode`, `BattingStyle`, `BowlingStyle`, `PlayerRole`) are optional columns on `Users`.
- Removed the `Player` and `Umpire` domain entities, EF configurations, repositories, and role-profile activation flow.
- `MatchPlayer.UserId` points directly to `Users.Id`; the unique `(MatchId, UserId)` index prevents the same account occupying multiple roster slots across both teams.
- `Match.CreatedByUserId` records the creator/organizer and scoring actions validate against that same user ID.
- `PlayerStatistics.UserId` and organizer statistics `UserId` now point directly to `Users.Id`. The existing SQL table name `UmpireStatistics` is retained in this migration to preserve existing statistics data.
- `GET` and `PUT /api/AccountProfile/{userId}` return/update one unified account profile including optional cricket details.
- Registration can omit a password. When omitted, the backend stores a hash of a random secret rather than an empty password hash. Such an account needs a password-reset flow before password-based login; match operations themselves do not require login in this prototype.
- Removed the roster-validation `[Authorize]` dependency and changed that request to carry `CreatedByUserId`, consistent with the no-JWT prototype requirement.
- Existing scoring logic, innings state, ball snapshots/undo, wickets, match results, player statistics calculations, lifecycle processing, and seven-day match cleanup were preserved while updating identity references.

## Database migration

A new `20261016180000_UnifyUserIdentity` migration:

1. Adds optional cricket-profile columns to `Users`.
2. Copies existing cricket-profile values from `Players` into their corresponding `Users` rows.
3. Re-points match roster and statistics foreign keys directly to `Users`.
4. Renames the match creator and user ID columns without changing the GUID values.
5. Drops the obsolete `Players` and `Umpires` tables only after dependent foreign keys have been removed.

**Back up the database before applying the migration.** From the solution directory, run:

```powershell
dotnet ef database update --project CricPulse.Infrastructure --startup-project CricPulse.API
```

If the EF command is not installed, install/restore the project's normal EF Core tooling first. Do not delete the existing migrations; the new migration depends on their history.

## Important security limitation

This backend intentionally has no JWT/authentication middleware, per the current prototype requirement. A caller-supplied `UserId` is not proof of account ownership and can be spoofed by a client. Before production/public deployment, organizer-only operations must be protected by real authentication/authorization.

## Validation status

The solution was statically checked for balanced C# delimiters, obsolete Player/Umpire entity references, and project configuration XML. A .NET build/test could not be run in the current execution environment because the .NET SDK is not installed here. Please run `dotnet build CricPulse.sln` locally before applying the migration.
