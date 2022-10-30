if (!$heroicons) { throw "heroicons should be set to the root path of where you cloned the heroicons repo"}
Push-Location $heroicons
try {
  git pull
}
finally {
  Pop-Location
}
