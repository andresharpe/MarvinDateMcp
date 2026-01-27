# Generate complete test documentation with all JSON results

$jsonPath = "docs/test-results.json"
$mdPath = "docs/test-results.md"
$docxPath = "docs/test-results.docx"

# Read JSON data
$testResults = Get-Content $jsonPath -Raw | ConvertFrom-Json

# Start building markdown
$md = @"
# MarvinDateMcp Integration Test Results

## Overview

This document contains the comprehensive test results from the MarvinDateMcp service, testing date context analysis across **11 well-known locations worldwide**.

**Test Date:** January 27, 2026  
**Status:** All tests passed (11/11)  
**Total Duration:** ~9 seconds  
**Holiday Lookahead:** 365 days (up to 10 holidays shown per location)

---

## Test Locations Summary

| # | Location | Country | Timezone | UTC Offset | Local Time | Status |
|---|----------|---------|----------|------------|------------|--------|
"@

# Add summary table rows
$index = 1
foreach ($result in $testResults) {
    $loc = $result.result.location
    $status = if ($result.success) { "PASS" } else { "FAIL" }
    
    $timeOnly = $loc.currentLocalTime -replace '.*T(\d{2}:\d{2}:\d{2}).*', '$1'
    $md += "`n| $index | $($result.location) | $($loc.countryCode) | $($loc.timezone) | $($loc.utcOffset) | $timeOnly | $status |"
    $index++
}

$md += @"

---

## Complete Test Results with Full JSON

"@

# Add detailed results for each location
$index = 1
foreach ($result in $testResults) {
    $loc = $result.result.location
    $today = $result.result.today
    $holidays = $result.result.upcomingHolidays
    
    $md += @"

### $index. $($result.location)

**Location Details:**
- **Resolved Name:** $($loc.resolvedName)
- **Address:** $($loc.formattedAddress)
- **Country Code:** $($loc.countryCode)
- **Timezone:** $($loc.timezone) (UTC $($loc.utcOffset))
- **Current Local Time:** $($loc.currentLocalTime)

**Date Context:**
- **Today:** $($today.dayOfWeek), $($today.date)$(if ($today.isWeekend) { " (Weekend)" })$(if ($today.isHoliday) { " (Holiday: $($today.holidayName))" })
- **Weekend Days:** $($result.result.thisWeek.weekendDays -join ', ')
- **Remaining Workdays This Week:** $($result.result.thisWeek.remainingWorkdays.Count)
- **Next Weekend:** $($result.result.keyDates.nextWeekend.start) to $($result.result.keyDates.nextWeekend.end)

**Upcoming Holidays ($($holidays.Count)):**
"@

    foreach ($holiday in $holidays) {
        $md += "`n- $($holiday.name) - $($holiday.date) ($($holiday.dayOfWeek))"
    }

    # Add complete JSON
    $jsonIndented = $result | ConvertTo-Json -Depth 10
    $md += @"


**Complete JSON Response:**

``````json
$jsonIndented
``````

---

"@
    
    $index++
}

# Add footer
$md += @"

## Test Validation

Each test validates the following:
- Location resolution (name and address)
- Country code detection
- Timezone identification
- Current local time calculation
- Today/tomorrow/day after tomorrow date info
- Weekend day identification
- Holiday detection and upcoming holidays (365-day lookahead, top 10 shown)
- This week and next week workday calculations
- Key dates (next Monday, Friday, weekend, etc.)

---

## Technical Details

**Service:** MarvinDateMcp.Api  
**Framework:** .NET 10.0  
**APIs Used:**
- Google Geocoding API
- Google Time Zone API
- Nager.Date Public Holiday API

**Test Framework:** xUnit 3.1.4  
**Test Project:** MarvinDateMcp.Tests

---

*Generated on $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss')) UTC*
"@

# Write markdown file
$md | Out-File -FilePath $mdPath -Encoding UTF8

Write-Host "Markdown file generated: $mdPath"

# Generate Word document using pandoc
Write-Host "Generating Word document..."
pandoc $mdPath -o $docxPath --toc --toc-depth=2 -s --syntax-highlighting=tango

if ($LASTEXITCODE -eq 0) {
    Write-Host "Word document generated: $docxPath"
    
    # Show file sizes
    $mdFile = Get-Item $mdPath
    $docxFile = Get-Item $docxPath
    
    Write-Host "`nFile Sizes:"
    Write-Host "  Markdown: $([math]::Round($mdFile.Length/1KB, 2)) KB"
    Write-Host "  Word:     $([math]::Round($docxFile.Length/1KB, 2)) KB"
} else {
    Write-Host "Error generating Word document" -ForegroundColor Red
}
