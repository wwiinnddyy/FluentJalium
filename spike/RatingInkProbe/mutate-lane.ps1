$ErrorActionPreference = 'Stop'
$path = 'C:\git\Jalium\FluentJalium\src\FluentJalium\Controls\Status\FluentRatingControl.cs'
$text = [System.IO.File]::ReadAllText($path)
$lane = "        var lane = new StackPanel { Orientation = Orientation.Horizontal };`n        lane.Children.Add(item);`n        host.Children.Add(lane);"
if ($text.IndexOf($lane) -lt 0) { throw 'lane block not found - refusing to mutate' }
$mutant = "        host.Children.Add(item); // MUTANT-98B"
$text = $text.Replace($lane, $mutant)
[System.IO.File]::WriteAllText($path, $text)
$check = [System.IO.File]::ReadAllText($path)
if ($check.IndexOf('MUTANT-98B') -lt 0) { throw 'mutant marker absent after write' }
if ($check.IndexOf('new StackPanel { Orientation = Orientation.Horizontal }') -ge 0) { throw 'lane still present after mutate' }
Write-Host 'mutated: lane removed'
