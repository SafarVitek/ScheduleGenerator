# ScheduleGenerator

ScheduleGenerator is a **C# console application** that converts a CSV schedule into multiple JSON schedules without collisions. The generated JSON is compatible with [kubosh.net](https://kubosh.net) for visualization.

---

## Features

- Reads a CSV file containing schedule entries.
- Generates **all possible schedules without time conflicts**.
- Supports **semester and year parameters**.
- Automatically assigns **distinct colors to subjects**.
- Optional **target number of free days** per week.
- Outputs multiple JSON files in a structured format.

---

## CSV Input Format

The CSV file should have the following columns:

| Code | Title | Day | From | To | Weeks | Room |
|------|-------|-----|------|----|-------|------|

- **Code** – Unique subject code.  
- **Title** – Subject title in English.  
- **Day** – Day of the week (`Mon`, `Tue`, `Wed`, `Thu`, `Fri`).  
- **From/To** – Time in `HH:mm` format.  
- **Weeks** – Optional week info.  
- **Room** – Room name.

### Example CSV

```csv
Code,Title,Day,From,To,Weeks,Room
AJ1101,Practical Language,Mon,07:00,10:00,,Room101
AJ1203,Grammar,Tue,09:00,11:00,,Room102
AJ1201,Phonetics,Wed,08:00,10:00,,Room103
PED1010,Pedagogy,Thu,10:00,12:00,,Room104
TI1001,Graphics,Fri,11:00,13:00,,Room105
TI1002,Materials,Mon,13:00,15:00,,Room106
TI1003,Data Processing,Tue,14:00,16:00,,Room107
```

---

## Example usage

Build and run the project with [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0):

```bash
dotnet run -- -i test-input.csv -s winter -y 2025 --free-days 2
```

### Parameters

| Flag | Description                                            | Required | Default |
|------|--------------------------------------------------------|----------|---------|
| `-i` | Path to input CSV file                                 | Yes      | N/A     |
| `-s` | Semester (e.g., `winter`)                              | Yes      | N/A     |
| `-y` | Year (e.g., `2025`)                                    | Yes      | N/A     |
| `--free-days` | Optional target number of free days per week (0 - 5)   | No       | 0       |

---

## Output

- JSON files are saved in the `output/` folder.
- Each file contains a valid, non-conflicting schedule.
- The JSON follows this format:

```json
{
  "sem": "winter",
  "studies": [],
  "grades": [],
  "subjects": [],
  "custom": [
    {
      "id": "CUST_1234567890",
      "name": "Practical Language",
      "link": "AJ1101-Practical Language",
      "day": 0,
      "week": "",
      "from": 0,
      "to": 3,
      "type": "custom",
      "custom_color": "green",
      "rooms": ["Room101"],
      "info": "",
      "layer": 1,
      "selected": false,
      "deleted": false
    }
  ],
  "selected": [],
  "deleted": [],
  "year": 2025
}
```

> These JSON files can be directly used on [kubosh.net](https://kubosh.net) for visualization.

---

## Notes

- The program automatically cycles colors for subjects if the number of subjects exceeds the available colors
- Days and times are normalized to match kubosh.net requirements
- Ensure CSV is properly formatted

---

## License

MIT License – feel free to use and modify.
