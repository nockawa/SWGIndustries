# SWG Industries - Design file

## Purpose
SWG Industries is a small WebApp that allows players to track their work efficiently.

Core features:
- Declare a stock of harvesters
- Retrieve current spawned resources from SWGAide
- Track lot allocation for Characters
- Track harvesting of a given resource for a given character with a given harvester
- Notify when a given resource is depleted, when a harvester is full, no longer powered, no longer with maintenance

## Terminology
- `Account`, a SWGIndustry account, related to the user who use the webapp, not related with the game.
- `SWGAccount`, the SWG Restoration III game account, for info purpose, we are not connected to it, we don't know/ask the password.
- `SWGCharacter`, each account can have up to four playable character.
- `SWGBuilding`, a lot based structure (house, harvester or factory).
- `Resource Category`, in game resources are each belonging to a given category, there's a tree of category which is fix defined.
- `PutDown` / `Redeed`, any lot based entity are represented by an item (a deed), which can be "consume" when we install it somewhere in a planet. The action of installing it is called "putdown". The action of getting back the entity to its deed (and storing it in the character's inventory) is called "redeed".
- `Lot`, in the game, each character has 10 lots to his/her disposal to put-down buildings.

## Data Models

### SWGAccount
Represents an account of SWG Restoration III.

- ID (int, PK)
- Name (string), of the account

### SWGCharacter
Represents an in-game character

- ID (int, PK)
- SWGAccountId (int, FK), owner of the character
- Name (string), of the in-game character

### SWGResource
Represents a given resource.

- ID (int, PK)
- Name (string), of the resource
- CategoryIndex (int), category the resource belongs to (see resource_tree.xml)
- SpawnDateTime (datetime)
- DepletedDateTime (datetime?), can be null.

### Cluster
A cluster is for controlling/monitoring multiple harvesters put on a given resource as a single entity. An action on the cluster will be applied on all harvesters belonging to it, info will be an accumulation. A cluster can have harvester of different characters but of the same SWGAccount.

- ID (int, PK)
- SWGAccountID (int, FK), owner SWG Restoration III account
- IsDefault (bool), each SWGAccount has a default cluster for buildings that don't belong to any particular one.
- Name (string), for info purpose
- Comment (string), for info purpose
- Planet (int), planet where the cluster is located

### SWGBuilding
Represent an in-game building (house, harvester or factory).

- ID (int, PK)
- SWGAccountID (int, FK), owner SWG Restoration III account
- Type (int), enum of House, Harvester, Factory
- SubType (int), type of house, harvester or factory (with inference of how many lots it consumes)
- CharacterID (int, FK), owner of the building (if applicable)
- ClusterID (int, FK), owner Cluster
- Name (string), for information purpose
- State (int), either Deed or PutDown
- PutDownPlanet (int), enum, for info purpose
- MaintenanceAmount (int), total maintenance amount since the last deposit/update
- MaintenanceLastUpdate (datetime), date/time of the last maintenance deposit/update
- PowerAmount (int), total power amount since the last deposit/update
- PowerLastUpdate (datetime), date/time of the last power deposit/update
- RunningMode (bool), for factory and harvester, on or off
- LastRunningDateTime (datetime?), last time, if any, the building was or is running
- LastStoppedDateTime (datetime), last time the building was stopped
- HarvesterBER (int), harvester BER
- HarvesterCapacity (int), harvester capacity
- HarvesterResourceID (int, FK), ID of the resource the harvester is retrieving
- HarvesterResourceConcentration (TODO check name), float number from 0 to 1 (%tage)
- HarvesterAmount, contains the accumulated amount for all previous cycles. When the harvester is stopped, this field is updated with the added amount that has been harvested for this cycle. This field + the computation of the amount of the current cycle (if running) will give the total amount harvested to compute the hopper content size.
- HarvesterAmountLastUpdate (datetime?), last time we updated the HarvesterAmount (use to computed total amount)

## Site navigation, pages

### / Account
SWGIndustries account info and operations, subtree to be defined.

### / SWGAccount
Page listing the accounts, their characters (up to four per account) with summary info for each character. With CRUD operations.

### / SWGHouse
List houses, with CRUD.

### / SWGFactory
List Factories, with CRUD.

### / SWGHarvester
Main page

#### / SWGHarvester / Inventory

List all the harvesters, which SWGAccount owns each, with property summary. In this page we don't care which character actually owns it.
- CRUD operations
- Possibility to clone an existing harvester (type, BER, Capacity kept)

#### / SWGHarvester / Operating

List all harvesters put down, grouped by their respective cluster. First cluster is the default one.
CRUD on clusters.
Add/remove harvester to a given cluster.
Operation on cluster: empty hopper, add maintenance, add power, destroy cluster
 


























