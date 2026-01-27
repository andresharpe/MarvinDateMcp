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
| 1 | Houses of Parliament, Westminster, London | GB | Europe/London | +00 | 01/27/2026 13:26:56 | PASS |
| 2 | White House, Washington DC | US | America/New_York | -05 | 01/27/2026 08:26:56 | PASS |
| 3 | Eiffel Tower, Paris | FR | Europe/Paris | +01 | 01/27/2026 14:26:57 | PASS |
| 4 | Tokyo Tower, Tokyo | JP | Asia/Tokyo | +09 | 01/27/2026 22:26:58 | PASS |
| 5 | Sydney Opera House, Sydney | AU | Australia/Sydney | +11 | 01/28/2026 00:26:59 | PASS |
| 6 | Brandenburg Gate, Berlin | DE | Europe/Berlin | +01 | 01/27/2026 14:26:59 | PASS |
| 7 | Red Square, Moscow | RU | Europe/Moscow | +03 | 01/27/2026 16:27:00 | PASS |
| 8 | Christ the Redeemer, Rio de Janeiro | BR | America/Sao_Paulo | -03 | 01/27/2026 10:27:01 | PASS |
| 9 | CN Tower, Toronto | CA | America/Toronto | -05 | 01/27/2026 08:27:02 | PASS |
| 10 | Regus Central Station Brussels | BE | Europe/Brussels | +01 | 01/27/2026 14:27:02 | PASS |
| 11 | Bleicherweg 10, Zurich | CH | Europe/Zurich | +01 | 01/27/2026 14:27:03 | PASS |
---

## Complete Test Results with Full JSON

### 1. Houses of Parliament, Westminster, London

**Location Details:**
- **Resolved Name:** Houses of Parliament, Westminster, London
- **Address:** London SW1A 0AA, UK
- **Country Code:** GB
- **Timezone:** Europe/London (UTC +00)
- **Current Local Time:** 01/27/2026 13:26:56

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Saint Patrick's Day - 2026-03-17 (Tuesday)
- Good Friday - 2026-04-03 (Friday)
- Easter Monday - 2026-04-06 (Monday)
- Early May Bank Holiday - 2026-05-04 (Monday)
- Spring Bank Holiday - 2026-05-25 (Monday)
- Battle of the Boyne - 2026-07-13 (Monday)
- Summer Bank Holiday - 2026-08-03 (Monday)
- Summer Bank Holiday - 2026-08-31 (Monday)
- Saint Andrew's Day - 2026-11-30 (Monday)
- Christmas Day - 2026-12-25 (Friday)

**Complete JSON Response:**

```json
{
  "location": "Houses of Parliament, Westminster, London",
  "expectedCountryCode": "GB",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Houses of Parliament, Westminster, London",
      "formattedAddress": "London SW1A 0AA, UK",
      "countryCode": "GB",
      "timezone": "Europe/London",
      "utcOffset": "+00",
      "currentLocalTime": "2026-01-27T13:26:56"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-03-17",
        "name": "Saint Patrick's Day",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-04",
        "name": "Early May Bank Holiday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-25",
        "name": "Spring Bank Holiday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-07-13",
        "name": "Battle of the Boyne",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-08-03",
        "name": "Summer Bank Holiday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-08-31",
        "name": "Summer Bank Holiday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-11-30",
        "name": "Saint Andrew's Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 2. White House, Washington DC

**Location Details:**
- **Resolved Name:** White House, Washington DC
- **Address:** 1600 Pennsylvania Ave NW, Washington, DC 20500, USA
- **Country Code:** US
- **Timezone:** America/New_York (UTC -05)
- **Current Local Time:** 01/27/2026 08:26:56

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Presidents Day - 2026-02-16 (Monday)
- Memorial Day - 2026-05-25 (Monday)
- Juneteenth National Independence Day - 2026-06-19 (Friday)
- Independence Day - 2026-07-03 (Friday)
- Labour Day - 2026-09-07 (Monday)
- Veterans Day - 2026-11-11 (Wednesday)
- Thanksgiving Day - 2026-11-26 (Thursday)
- Christmas Day - 2026-12-25 (Friday)
- New Year's Day - 2027-01-01 (Friday)
- Martin Luther King, Jr. Day - 2027-01-18 (Monday)

**Complete JSON Response:**

```json
{
  "location": "White House, Washington DC",
  "expectedCountryCode": "US",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "White House, Washington DC",
      "formattedAddress": "1600 Pennsylvania Ave NW, Washington, DC 20500, USA",
      "countryCode": "US",
      "timezone": "America/New_York",
      "utcOffset": "-05",
      "currentLocalTime": "2026-01-27T08:26:56"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-02-16",
        "name": "Presidents Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-25",
        "name": "Memorial Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-06-19",
        "name": "Juneteenth National Independence Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-07-03",
        "name": "Independence Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-09-07",
        "name": "Labour Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-11-11",
        "name": "Veterans Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-11-26",
        "name": "Thanksgiving Day",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2027-01-01",
        "name": "New Year's Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2027-01-18",
        "name": "Martin Luther King, Jr. Day",
        "dayOfWeek": "Monday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 3. Eiffel Tower, Paris

**Location Details:**
- **Resolved Name:** Eiffel Tower, Paris
- **Address:** Av. Gustave Eiffel, 75007 Paris, France
- **Country Code:** FR
- **Timezone:** Europe/Paris (UTC +01)
- **Current Local Time:** 01/27/2026 14:26:57

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Easter Monday - 2026-04-06 (Monday)
- Labour Day - 2026-05-01 (Friday)
- Victory in Europe Day - 2026-05-08 (Friday)
- Ascension Day - 2026-05-14 (Thursday)
- Whit Monday - 2026-05-25 (Monday)
- Bastille Day - 2026-07-14 (Tuesday)
- Assumption Day - 2026-08-15 (Saturday)
- All Saints' Day - 2026-11-01 (Sunday)
- Armistice Day - 2026-11-11 (Wednesday)
- Christmas Day - 2026-12-25 (Friday)

**Complete JSON Response:**

```json
{
  "location": "Eiffel Tower, Paris",
  "expectedCountryCode": "FR",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Eiffel Tower, Paris",
      "formattedAddress": "Av. Gustave Eiffel, 75007 Paris, France",
      "countryCode": "FR",
      "timezone": "Europe/Paris",
      "utcOffset": "+01",
      "currentLocalTime": "2026-01-27T14:26:57"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-08",
        "name": "Victory in Europe Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-14",
        "name": "Ascension Day",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-05-25",
        "name": "Whit Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-07-14",
        "name": "Bastille Day",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-08-15",
        "name": "Assumption Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-11-01",
        "name": "All Saints' Day",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-11-11",
        "name": "Armistice Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 4. Tokyo Tower, Tokyo

**Location Details:**
- **Resolved Name:** Tokyo Tower, Tokyo
- **Address:** 4-chōme-2-8 Shibakōen, Minato City, Tokyo 105-0011, Japan
- **Country Code:** JP
- **Timezone:** Asia/Tokyo (UTC +09)
- **Current Local Time:** 01/27/2026 22:26:58

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Foundation Day - 2026-02-11 (Wednesday)
- The Emperor's Birthday - 2026-02-23 (Monday)
- Vernal Equinox Day - 2026-03-20 (Friday)
- Shōwa Day - 2026-04-29 (Wednesday)
- Constitution Memorial Day - 2026-05-04 (Monday)
- Greenery Day - 2026-05-04 (Monday)
- Children's Day - 2026-05-05 (Tuesday)
- Marine Day - 2026-07-20 (Monday)
- Mountain Day - 2026-08-11 (Tuesday)
- Respect for the Aged Day - 2026-09-21 (Monday)

**Complete JSON Response:**

```json
{
  "location": "Tokyo Tower, Tokyo",
  "expectedCountryCode": "JP",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Tokyo Tower, Tokyo",
      "formattedAddress": "4-chōme-2-8 Shibakōen, Minato City, Tokyo 105-0011, Japan",
      "countryCode": "JP",
      "timezone": "Asia/Tokyo",
      "utcOffset": "+09",
      "currentLocalTime": "2026-01-27T22:26:58"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-02-11",
        "name": "Foundation Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-02-23",
        "name": "The Emperor's Birthday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-03-20",
        "name": "Vernal Equinox Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-29",
        "name": "Shōwa Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-05-04",
        "name": "Constitution Memorial Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-04",
        "name": "Greenery Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-05",
        "name": "Children's Day",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-07-20",
        "name": "Marine Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-08-11",
        "name": "Mountain Day",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-09-21",
        "name": "Respect for the Aged Day",
        "dayOfWeek": "Monday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 5. Sydney Opera House, Sydney

**Location Details:**
- **Resolved Name:** Sydney Opera House, Sydney
- **Address:** Bennelong Point, Sydney NSW 2000, Australia
- **Country Code:** AU
- **Timezone:** Australia/Sydney (UTC +11)
- **Current Local Time:** 01/28/2026 00:26:59

**Date Context:**
- **Today:** Wednesday, 2026-01-28
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 3
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Good Friday - 2026-04-03 (Friday)
- Holy Saturday - 2026-04-04 (Saturday)
- Easter Sunday - 2026-04-05 (Sunday)
- Easter Monday - 2026-04-06 (Monday)
- Anzac Day - 2026-04-25 (Saturday)
- King's Birthday - 2026-06-08 (Monday)
- Labour Day - 2026-10-05 (Monday)
- Christmas Day - 2026-12-25 (Friday)
- St. Stephen's Day - 2026-12-28 (Monday)
- New Year's Day - 2027-01-01 (Friday)

**Complete JSON Response:**

```json
{
  "location": "Sydney Opera House, Sydney",
  "expectedCountryCode": "AU",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Sydney Opera House, Sydney",
      "formattedAddress": "Bennelong Point, Sydney NSW 2000, Australia",
      "countryCode": "AU",
      "timezone": "Australia/Sydney",
      "utcOffset": "+11",
      "currentLocalTime": "2026-01-28T00:26:59"
    },
    "today": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-30",
      "dayOfWeek": "Friday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-04",
        "name": "Holy Saturday",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-04-05",
        "name": "Easter Sunday",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-04-25",
        "name": "Anzac Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-06-08",
        "name": "King's Birthday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-10-05",
        "name": "Labour Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-12-28",
        "name": "St. Stephen's Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2027-01-01",
        "name": "New Year's Day",
        "dayOfWeek": "Friday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-02-04",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 6. Brandenburg Gate, Berlin

**Location Details:**
- **Resolved Name:** Brandenburg Gate, Berlin
- **Address:** Pariser Platz, 10117 Berlin, Germany
- **Country Code:** DE
- **Timezone:** Europe/Berlin (UTC +01)
- **Current Local Time:** 01/27/2026 14:26:59

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- International Women's Day - 2026-03-08 (Sunday)
- Good Friday - 2026-04-03 (Friday)
- Easter Monday - 2026-04-06 (Monday)
- Labour Day - 2026-05-01 (Friday)
- Ascension Day - 2026-05-14 (Thursday)
- Whit Monday - 2026-05-25 (Monday)
- German Unity Day - 2026-10-03 (Saturday)
- Christmas Day - 2026-12-25 (Friday)
- St. Stephen's Day - 2026-12-26 (Saturday)
- New Year's Day - 2027-01-01 (Friday)

**Complete JSON Response:**

```json
{
  "location": "Brandenburg Gate, Berlin",
  "expectedCountryCode": "DE",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Brandenburg Gate, Berlin",
      "formattedAddress": "Pariser Platz, 10117 Berlin, Germany",
      "countryCode": "DE",
      "timezone": "Europe/Berlin",
      "utcOffset": "+01",
      "currentLocalTime": "2026-01-27T14:26:59"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-03-08",
        "name": "International Women's Day",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-14",
        "name": "Ascension Day",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-05-25",
        "name": "Whit Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-10-03",
        "name": "German Unity Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-12-26",
        "name": "St. Stephen's Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2027-01-01",
        "name": "New Year's Day",
        "dayOfWeek": "Friday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 7. Red Square, Moscow

**Location Details:**
- **Resolved Name:** Red Square, Moscow
- **Address:** Krasnaya ploshad, Moskva, Russia, 109012
- **Country Code:** RU
- **Timezone:** Europe/Moscow (UTC +03)
- **Current Local Time:** 01/27/2026 16:27:00

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Defender of the Fatherland Day - 2026-02-23 (Monday)
- International Women's Day - 2026-03-08 (Sunday)
- Labour Day - 2026-05-01 (Friday)
- Victory Day - 2026-05-09 (Saturday)
- Russia Day - 2026-06-12 (Friday)
- Unity Day - 2026-11-04 (Wednesday)
- New Year's Day - 2027-01-01 (Friday)
- New Year holiday - 2027-01-02 (Saturday)
- New Year holiday - 2027-01-03 (Sunday)
- New Year holiday - 2027-01-04 (Monday)

**Complete JSON Response:**

```json
{
  "location": "Red Square, Moscow",
  "expectedCountryCode": "RU",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Red Square, Moscow",
      "formattedAddress": "Krasnaya ploshad, Moskva, Russia, 109012",
      "countryCode": "RU",
      "timezone": "Europe/Moscow",
      "utcOffset": "+03",
      "currentLocalTime": "2026-01-27T16:27:00"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-02-23",
        "name": "Defender of the Fatherland Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-03-08",
        "name": "International Women's Day",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-09",
        "name": "Victory Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-06-12",
        "name": "Russia Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-11-04",
        "name": "Unity Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2027-01-01",
        "name": "New Year's Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2027-01-02",
        "name": "New Year holiday",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2027-01-03",
        "name": "New Year holiday",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2027-01-04",
        "name": "New Year holiday",
        "dayOfWeek": "Monday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 8. Christ the Redeemer, Rio de Janeiro

**Location Details:**
- **Resolved Name:** Christ the Redeemer, Rio de Janeiro
- **Address:** Parque Nacional da Tijuca - Alto da Boa Vista, Rio de Janeiro - RJ, 22261, Brazil
- **Country Code:** BR
- **Timezone:** America/Sao_Paulo (UTC -03)
- **Current Local Time:** 01/27/2026 10:27:01

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Carnival - 2026-02-16 (Monday)
- Carnival - 2026-02-17 (Tuesday)
- Good Friday - 2026-04-03 (Friday)
- Easter Sunday - 2026-04-05 (Sunday)
- Tiradentes - 2026-04-21 (Tuesday)
- Labour Day - 2026-05-01 (Friday)
- Corpus Christi - 2026-06-04 (Thursday)
- Independence Day - 2026-09-07 (Monday)
- Our Lady of Aparecida - 2026-10-12 (Monday)
- All Souls' Day - 2026-11-02 (Monday)

**Complete JSON Response:**

```json
{
  "location": "Christ the Redeemer, Rio de Janeiro",
  "expectedCountryCode": "BR",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Christ the Redeemer, Rio de Janeiro",
      "formattedAddress": "Parque Nacional da Tijuca - Alto da Boa Vista, Rio de Janeiro - RJ, 22261, Brazil",
      "countryCode": "BR",
      "timezone": "America/Sao_Paulo",
      "utcOffset": "-03",
      "currentLocalTime": "2026-01-27T10:27:01"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-02-16",
        "name": "Carnival",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-02-17",
        "name": "Carnival",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-05",
        "name": "Easter Sunday",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-04-21",
        "name": "Tiradentes",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-06-04",
        "name": "Corpus Christi",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-09-07",
        "name": "Independence Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-10-12",
        "name": "Our Lady of Aparecida",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-11-02",
        "name": "All Souls' Day",
        "dayOfWeek": "Monday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 9. CN Tower, Toronto

**Location Details:**
- **Resolved Name:** CN Tower, Toronto
- **Address:** 290 Bremner Blvd, Toronto, ON M5V 3L9, Canada
- **Country Code:** CA
- **Timezone:** America/Toronto (UTC -05)
- **Current Local Time:** 01/27/2026 08:27:02

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Family Day - 2026-02-16 (Monday)
- Good Friday - 2026-04-03 (Friday)
- Victoria Day - 2026-05-18 (Monday)
- Canada Day - 2026-07-01 (Wednesday)
- Civic Holiday - 2026-08-03 (Monday)
- Labour Day - 2026-09-07 (Monday)
- National Day for Truth and Reconciliation - 2026-09-30 (Wednesday)
- Thanksgiving - 2026-10-12 (Monday)
- Christmas Day - 2026-12-25 (Friday)
- St. Stephen's Day - 2026-12-26 (Saturday)

**Complete JSON Response:**

```json
{
  "location": "CN Tower, Toronto",
  "expectedCountryCode": "CA",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "CN Tower, Toronto",
      "formattedAddress": "290 Bremner Blvd, Toronto, ON M5V 3L9, Canada",
      "countryCode": "CA",
      "timezone": "America/Toronto",
      "utcOffset": "-05",
      "currentLocalTime": "2026-01-27T08:27:02"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-02-16",
        "name": "Family Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-18",
        "name": "Victoria Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-07-01",
        "name": "Canada Day",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-08-03",
        "name": "Civic Holiday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-09-07",
        "name": "Labour Day",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-09-30",
        "name": "National Day for Truth and Reconciliation",
        "dayOfWeek": "Wednesday"
      },
      {
        "date": "2026-10-12",
        "name": "Thanksgiving",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-12-26",
        "name": "St. Stephen's Day",
        "dayOfWeek": "Saturday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 10. Regus Central Station Brussels

**Location Details:**
- **Resolved Name:** Regus Central Station Brussels
- **Address:** Central Station, Rue des Colonies 11, 1000 Bruxelles, Belgium
- **Country Code:** BE
- **Timezone:** Europe/Brussels (UTC +01)
- **Current Local Time:** 01/27/2026 14:27:02

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Good Friday - 2026-04-03 (Friday)
- Easter Sunday - 2026-04-05 (Sunday)
- Easter Monday - 2026-04-06 (Monday)
- Labour Day - 2026-05-01 (Friday)
- Ascension Day - 2026-05-14 (Thursday)
- Day after Ascension Day - 2026-05-15 (Friday)
- Whit Monday - 2026-05-25 (Monday)
- Belgian National Day - 2026-07-21 (Tuesday)
- Assumption Day - 2026-08-15 (Saturday)
- All Saints' Day - 2026-11-01 (Sunday)

**Complete JSON Response:**

```json
{
  "location": "Regus Central Station Brussels",
  "expectedCountryCode": "BE",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Regus Central Station Brussels",
      "formattedAddress": "Central Station, Rue des Colonies 11, 1000 Bruxelles, Belgium",
      "countryCode": "BE",
      "timezone": "Europe/Brussels",
      "utcOffset": "+01",
      "currentLocalTime": "2026-01-27T14:27:02"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-05",
        "name": "Easter Sunday",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-14",
        "name": "Ascension Day",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-05-15",
        "name": "Day after Ascension Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-25",
        "name": "Whit Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-07-21",
        "name": "Belgian National Day",
        "dayOfWeek": "Tuesday"
      },
      {
        "date": "2026-08-15",
        "name": "Assumption Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-11-01",
        "name": "All Saints' Day",
        "dayOfWeek": "Sunday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

### 11. Bleicherweg 10, Zurich

**Location Details:**
- **Resolved Name:** Bleicherweg 10, Zurich
- **Address:** Bleicherweg 10, 8002 Zürich, Switzerland
- **Country Code:** CH
- **Timezone:** Europe/Zurich (UTC +01)
- **Current Local Time:** 01/27/2026 14:27:03

**Date Context:**
- **Today:** Tuesday, 2026-01-27
- **Weekend Days:** Saturday, Sunday
- **Remaining Workdays This Week:** 4
- **Next Weekend:** 2026-01-31 to 2026-02-01

**Upcoming Holidays (10):**
- Good Friday - 2026-04-03 (Friday)
- Easter Monday - 2026-04-06 (Monday)
- Labour Day - 2026-05-01 (Friday)
- Ascension Day - 2026-05-14 (Thursday)
- Whit Monday - 2026-05-25 (Monday)
- Swiss National Day - 2026-08-01 (Saturday)
- Federal Day of Thanksgiving - 2026-09-20 (Sunday)
- Christmas Day - 2026-12-25 (Friday)
- St. Stephen's Day - 2026-12-26 (Saturday)
- New Year's Day - 2027-01-01 (Friday)

**Complete JSON Response:**

```json
{
  "location": "Bleicherweg 10, Zurich",
  "expectedCountryCode": "CH",
  "success": true,
  "result": {
    "location": {
      "resolvedName": "Bleicherweg 10, Zurich",
      "formattedAddress": "Bleicherweg 10, 8002 Zürich, Switzerland",
      "countryCode": "CH",
      "timezone": "Europe/Zurich",
      "utcOffset": "+01",
      "currentLocalTime": "2026-01-27T14:27:03"
    },
    "today": {
      "date": "2026-01-27",
      "dayOfWeek": "Tuesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "tomorrow": {
      "date": "2026-01-28",
      "dayOfWeek": "Wednesday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "dayAfterTomorrow": {
      "date": "2026-01-29",
      "dayOfWeek": "Thursday",
      "isWeekend": false,
      "isHoliday": false,
      "holidayName": null
    },
    "thisWeek": {
      "weekendDays": [
        "Saturday",
        "Sunday"
      ],
      "weekendDates": [
        "2026-01-31",
        "2026-02-01"
      ],
      "remainingWorkdays": [
        "2026-01-27",
        "2026-01-28",
        "2026-01-29",
        "2026-01-30"
      ]
    },
    "nextWeek": {
      "monday": "2026-02-02",
      "friday": "2026-02-06",
      "weekendDates": [
        "2026-02-07",
        "2026-02-08"
      ],
      "workdays": [
        "2026-02-02",
        "2026-02-03",
        "2026-02-04",
        "2026-02-05",
        "2026-02-06"
      ]
    },
    "upcomingHolidays": [
      {
        "date": "2026-04-03",
        "name": "Good Friday",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-04-06",
        "name": "Easter Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-05-01",
        "name": "Labour Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-05-14",
        "name": "Ascension Day",
        "dayOfWeek": "Thursday"
      },
      {
        "date": "2026-05-25",
        "name": "Whit Monday",
        "dayOfWeek": "Monday"
      },
      {
        "date": "2026-08-01",
        "name": "Swiss National Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2026-09-20",
        "name": "Federal Day of Thanksgiving",
        "dayOfWeek": "Sunday"
      },
      {
        "date": "2026-12-25",
        "name": "Christmas Day",
        "dayOfWeek": "Friday"
      },
      {
        "date": "2026-12-26",
        "name": "St. Stephen's Day",
        "dayOfWeek": "Saturday"
      },
      {
        "date": "2027-01-01",
        "name": "New Year's Day",
        "dayOfWeek": "Friday"
      }
    ],
    "keyDates": {
      "nextMonday": "2026-02-02",
      "nextTuesday": "2026-02-03",
      "nextWednesday": "2026-01-28",
      "nextThursday": "2026-01-29",
      "nextFriday": "2026-01-30",
      "nextSaturday": "2026-01-31",
      "nextSunday": "2026-02-01",
      "nextWeekend": {
        "start": "2026-01-31",
        "end": "2026-02-01"
      }
    }
  }
}
```

---

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

*Generated on 2026-01-27 14:33:50 UTC*
