# Buildings Included

Hi! seems you're interested in shipping your maps with some pre-built, ~~factory produced~~ eco-friendly buildings.

Well in this handy little guide here I'll tell you all about the things you need to add a chicken coop to your own farm map.

## The basics
This mod assumes you have at least a basic knowledge of [Content Patcher](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md), and JSON in general.

## How does it work
The mod provides a new asset, ``MindMeltMax.MapBuildings/Buildings``, a dictionary asset where the key is intended to be something unique (like your own mod id), and the value is a list of [MapBuilding objects](#the-format).

So all you'd need to do is add something like the following to your content.json file:
```json
{
	"Format": "2.3.0",
	"Changes": [
		...
		{
			"Action": "EditData",
			"Target": "MindMeltMax.MapBuildings/Buildings",
			"When": {
				"HasMod: |contains=MindMeltMax.MapBuildings": true
			},
			"Entries": {
				"{{ModId}}": [
					{
						"Location": "Farm",
						"X": 42,
						"Y": 15,
						"Building": "Shed"
					},
					{
						"Location": "Farm",
						"X": 12,
						"Y": 24,
						"Building": "Barn",
						"Animals": [
							"White Cow",
							"Brown Cow",
						]
					}
				]
			}
		}
	]
}
```

## The format
In the example above I explained how to use the asset, but that only gets you so far if you don't know what the fields mean.  
So here is an overview of all currently settable fields.

| Name | Description | Optional |
| ---- | ----------- | -------- |
| Location | The internal name of the location as it appears in ``Data\\Locations`` | No |
| X | The x tile position on the map where the building should be placed, this represents the top left corner of the building | No |
| Y | The y tile position on the map where the building should be placed, this represents the top left corner of the building | No |
| Building | The id of the building as it appears in ``Data\\Buildings`` | No |
| Animals | A list of animal id's (as they appear in ``Data\\Animals``) to add to the building, invalid animals will be ignored | Yes |

## Notes
Placed buildings ignore checking naturally spawned objects, they will however be blocked by player placed objects.

There is currently no option to replace a placed building with the mod, this will be added in the future.