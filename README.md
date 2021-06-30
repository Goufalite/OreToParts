# OreToParts
Mod for the game Kerbal Space Program to create parts from ore.

Mine ore.
Receive parts!

Introducing a minimalistic mod transforming all ore tanks into converters changing ore into vital parts such as repair kits, solar panels,...

2 modes are available

## Craft

The oretotanks.cfg file contains a comma-separated list of parts that can be crafted. The ore price to craft them is the mass of the part (5kg = 5 units of ore). The part will be stored in the ore tank’s new inventory, so keep an eye on it!
The converter can only craft cargo-type parts which have a volume. You can list them in the VAB by pressing the cargo-like button near the action-groups button.

## Duplicate

Place a part to duplicate in the first slot and press Duplicate! Same as above, the ore price is the mass of the part.

## Adding my mod’s parts/custom parts.

As far as I tested this, the mod manages unknown parts to craft by putting ??partName?? in the part description. If you want to add your own parts to craft, replace or concatenate the partList attribute with a MM patch. Parts must be comma separated without spaces.

    @PART[*]:HAS[@MODULE[OreToParts]]:AFTER[OreToParts]
    {
    	@MODULE[OreToParts] {
		    @partList = #$partList$,mycommaseparatedparts
    	}
    }


CC-BY-SA 4.0
Last release :
Github :
Issues :

## Dependencies :
* Module Manager (hard dependency) - install separately

## Instructions 

Remove previous versions. Unzip into the GameData folder of your KSP installation, your folder should look like GameData/OreToParts

Please provide a KSP.log file in order for me to help you.


Localization (please help) : English : OK, French : OK

## Changelog
###0.1.0.0
* initial release
* ore tanks can convert parts by either crafting them or duplicating them
* ore tanks have different maximum volume
* default parts to craft are repair kits, eva science kits, HG-5 antenna, 1x6 solar panel, eva jetpacks
