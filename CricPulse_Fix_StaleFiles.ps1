$ErrorActionPreference = 'Stop'
$root = Get-Location
$projectRoot = $root
if (-not (Test-Path (Join-Path $root 'CricPulse.sln'))) {
    if (Test-Path (Join-Path $root 'CricPulse\CricPulse.sln')) {
        $projectRoot = Join-Path $root 'CricPulse'
    } else {
        throw 'Run this script from the folder containing CricPulse.sln, or from its parent folder.'
    }
}
$relativeFiles = @(
    'CricPulse.API\Controllers\PlayerProfileController.cs',
    'CricPulse.API\Controllers\UmpireController.cs',
    'CricPulse.Application\DTOs\Player\CreatePlayerProfileDto.cs',
    'CricPulse.Application\DTOs\Player\PlayerProfileDto.cs',
    'CricPulse.Application\DTOs\Player\UpdatePlayerProfileDto.cs',
    'CricPulse.Application\DTOs\Umpire\UmpireProfileDto.cs',
    'CricPulse.Application\DTOs\Umpire\UmpireStatisticsDto.cs',
    'CricPulse.Application\Interfaces\Repositories\IPlayerRepository.cs',
    'CricPulse.Application\Interfaces\Repositories\IUmpireRepository.cs',
    'CricPulse.Application\Interfaces\Repositories\IUmpireStatisticsRepository.cs',
    'CricPulse.Application\Interfaces\Repositories\IUserProfileRepository.cs',
    'CricPulse.Application\Interfaces\Services\IPlayerProfileService.cs',
    'CricPulse.Application\Interfaces\Services\IPlayerService.cs',
    'CricPulse.Application\Interfaces\Services\IUmpireService.cs',
    'CricPulse.Application\Interfaces\Services\IUmpireStatisticsService.cs',
    'CricPulse.Application\Services\Player\PlayerProfileService.cs',
    'CricPulse.Application\Services\Umpire\UmpireService.cs',
    'CricPulse.Domain\Entities\Player.cs',
    'CricPulse.Domain\Entities\Umpire.cs',
    'CricPulse.Domain\Entities\UmpireStatistics.cs',
    'CricPulse.Infrastructure\Configurations\PlayerConfiguration.cs',
    'CricPulse.Infrastructure\Configurations\UmpireConfiguration.cs',
    'CricPulse.Infrastructure\Configurations\UmpireStatisticsConfiguration.cs',
    'CricPulse.Infrastructure\Repositories\PlayerRepository.cs',
    'CricPulse.Infrastructure\Repositories\UmpireRepository.cs',
    'CricPulse.Infrastructure\Repositories\UmpireStatisticsRepository.cs',
    'CricPulse.Infrastructure\Repositories\UserProfileRepository.cs'
)
$removed = 0
foreach ($relative in $relativeFiles) {
    $path = Join-Path $projectRoot $relative
    if (Test-Path $path) {
        Remove-Item -LiteralPath $path -Force
        Write-Host "Removed obsolete file: $relative"
        $removed++
    }
}
Write-Host "Removed $removed obsolete files. Existing migration files were not touched."
Write-Host 'Next: in Visual Studio, Build > Clean Solution, then Build > Rebuild Solution.'
