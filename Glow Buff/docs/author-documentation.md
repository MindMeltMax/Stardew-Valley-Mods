# Glow Buff Documentation

Glow buff provides a custom buff type which adds a glow ring like effect.

The following is some simple documentation on how to use it.

* [Get Started](#get-started)
	* [Basics](#basics)
	* [Fields](#fields)
* [Notes](#notes)
	* [Compatibility](#compatibility)
	* [Textures](#textures)
	* [Colors](#colors)

## Get Started

The mod is designed to be easy to integrate following the basic buff format as much as possible.

You don't need an extra content pack, everything can be added directly via C# or Content Patcher.

Glow buff is currently only applicable to food/drink items.

Before adding this buff, please read the [notes](#notes) section as it goes over known incompatibilities (if any) and some other usefull information.

### Basics

The buff is added directly to the list of buffs for any food/drink item, either as it's own standalone buff, or through `CustomFields` on an existing buff.

Here are some quick examples of the basic setup for adding the buff to an existing item with c# and content patcher:

**C#**
```cs
private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
{
	if (e.NameWithoutLocale.IsEquivalentTo("Data\\Objects"))
	{
		//Option A: The item doesn't already have buffs
		e.Edit(asset => 
		{
			var data = asset.AsDictionary<string, ObjectData>().Data;
			data["{{Your_ItemId}}"].Buffs ??= []; //Make sure the buffs list exists
			data["{{Your_ItemId}}"].Buffs.Add(new()
			{
				Duration = 120, //The duration of the buff in game minutes
				BuffId = "MindMeltMax.GlowBuff/Glow", //Must be this exact Id to load the correct buff
				CustomFields = new()
				{
					//Add the properties for the buff, will be explained in Fields
				}
			});
		});

		//Option B: The item already has buffs
		e.Edit(asset => 
		{
			var data = asset.AsDictionary<string, ObjectData>().Data;
			data["{{Your_ItemId}}"].Buffs[0].CustomFields ??= []; //Make sure CustomFields exists
			data["{{Your_ItemId}}"].Buffs[0].CustomFields.TryAddMany(new() 
			{
				//Add the properties for the buff
			});
		});
	}
}
```

**Content Patcher**
```json
{
  	"Format": "2.2.0",
  	"Changes": [
		//Option A: The item doesn't already have buffs
    	{
      		"Action": "EditData",
      		"Target": "Data/Objects",
      		"TargetField": [
        		"{{Your_ItemId}}"
      		],
      		"Entries": {
				"Buffs": [
					{
						"BuffId": "MindMeltMax.GlowBuff/Glow", //Must be this exact Id to load the correct buff
						"Duration": 120, //The duration of the buff in game minutes
						"CustomFields": {
							//Add the properties for the buff, will be explained in Fields
						}
					}
				]
      		}
    	},
		//Option B: The item already has buffs
		{
			"Action": "EditData",
			"Target": "Data/Objects",
			"TargetField": [
				"{{Your_ItemId}}",
				"CustomFields"
			],
			"Entries": {
				//Add the properties for the buff
			}
		}
  	]
}
```

### Fields

All customization for the glow effect is added through the `"CustomFields"` property of an objects/buffs data.

Every field key must start with **"MindMeltMax.GlowBuff/"**, this is to ensure compatibility with other mods.

By default the following fields are supported:

| Key | Description |
| --- | ----------- |
| `Glow` | Tells the mod to add the glow buff effect, this can be ommited when the buff's id is `"MindMeltMax.GlowBuff/Glow"` |
| `GlowTexture` / `Texture` | The id of the texture to load. A list of recognized values can be found [here](#textures). |
| `GlowRadius` / `Radius` | The radius of the glow effect. (Decimal values allowed) |
| `GlowColor` / `Color` | The rgba color value of the glow effect. (please see [notes](#colors) before using) |
| ~~`GlowDuration`~~ | **Removed in 2.0.0+** The duration in game minutes of the buff. When added to a buff's custom fields, will be overriden when the buffs duration is not default (-2). When ommited from ObjectData, will default to lasting the rest of the day |
| `DisplayName` | The translated name of the buff as it will appear in the objects buff list when hovered (if the buff is added to the objects buffs), and the name of the buff in the buffs display. |
| `Description` | The translated description of the buff as it will appear in the buffs display. |
| `HoverText` | The text to display in an item's hover box, by default this would be "+{{radius}} {{DisplayName}}" |

## Notes

### Compatibility

No known incompatibilities as of yet

### Textures

The game has a few different texture types for light sources, they are as follows:

| Id | Type |
| -- | ---- |
| 1 | the texture used by lanterns and glow rings |
| 2 | The texture used by televisions |
| 3 | Unused |
| 4 | (Default) The texture used by most light giving objects, fireflies, and junimo's in events (Recommended when using colored lights) |
| 5 | The texture used by regular ghosts |
| 6 | The texture used by the games default light points |
| 7 | The texture given of by the movie theater projector |
| 8 | The texture used by fish tanks |
| 9 | The texture used by the decorated trees during winter |
| 10 | The texture used for tiny light sources (like butterflies) |

### Colors

Due to how lighting colors are handled by the game, some colors show better than others. Keep this in mind when changing color values.

The format for colors can be something as follows:

* **"{{red}},{{green}},{{blue}}"**
* **"{{red}},{{green}},{{blue}},{{alpha}}"**

Additionally, a list of pre-defined color names can be used (Like "Red", "Blue", etc.). A full list of available pre-defined colors can be found [here](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html#properties) but if you prefer a visual, I recommend [this overview](https://gpoamusements.itch.io/monogame-colour-palette) by [Fergus Buckner](https://gpoamusements.itch.io/)

~~But who's to say another value isn't possible~~