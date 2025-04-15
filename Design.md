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
- `AppAccount`, a SWG Industry account, related to the user who use the webapp, not related with the game.
- `GameAccount`, the SWG Restoration III game account, for info purpose, we are not connected to it, we don't know/ask the password.
- `Character`, each account can have up to four playable character.
- `GBuilding`, a lot based structure (house, harvester or factory).
- `Resource Category`, in game resources are each belonging to a given category, there's a tree of category which is fix defined.
- `PutDown` / `Redeed`, any lot based entity are represented by an item (a deed), which can be "consume" when we install it somewhere in a planet. The action of installing it is called "putdown". The action of getting back the entity to its deed (and storing it in the character's inventory) is called "redeed".
- `Lot`, in the game, each character has 10 lots to his/her disposal to put-down buildings.
- `Crew`, a logical team of people sharing the same harvesting activity.

## Data Models

### AppAccount
Represent a registered account in SWG Industries

- `ID` (int, PK)
- `CorrelationId` (int), ID of the external identity provider (Discord or Google)
- `Name` (string), taken from the external authenticator
- App Settings, ThemeMode, ...
- `GameAccounts` (Nav List of `GameAccount.OwnerAppUser`)
- `Crew` (int, FK to `Crew` table)

### GameAccount
Represents an account of SWG Restoration III.

- `ID` (int, PK)
- `AppAccountOwner` (int, FK of `AppAccount`), which SWG Industries account owns that SWG Account
- `Name` (string), of the account

### Character
Represents an in-game character

- `ID` (int, PK)
- `GameAccount` (int, FK of `GameAccount`), owner of the character
- `Name` (string), of the in-game character
- `IsCrewMember` (bool), true if the character of this account is a member of the crew (only meaningful if `GameAccount.CrewLeader` is not `0`)

### Crew
Represent a crew of characters

- `ID` (int, PK)
- `Name` (string, unique)
- `CrewOwner` (AppAcount, FK)
- `CrewMembers` (Nav List of `AppAccount`)

### CrewInvitation
Handle crew invitation.

- `ID` (int, PK), a new invitation initiated
- `FromGameAccount` (int FK of `GameAccount`), account of the crew leader that initiated the invitation.
- `ToGameAccount` (int, FK of `GameAccount`), account that received the invitation.
- `InviteOrRequestToJoin` (bool), `true` if the leader invite a member, `false` if a user requests to join.
- `Status` (enum, Pending, Accepted, Rejected) curren status of the invitation.

### Resource
Represents a given resource.

- ID (int, PK)
- Name (string), of the resource
- CategoryIndex (int), category the resource belongs to (see resource_tree.xml)
- SpawnDateTime (datetime)
- DepletedDateTime (datetime?), can be null.

### Cluster
A cluster is for controlling/monitoring multiple harvesters put on a given resource as a single entity. An action on the cluster will be applied on all harvesters belonging to it, info will be an accumulation. A cluster can have harvester of different characters but of the same GameAccount.

- ID (int, PK)
- GameAccountID (int, FK), owner SWG Restoration III account
- IsDefault (bool), each GameAccount has a default cluster for buildings that don't belong to any particular one.
- Name (string), for info purpose
- Comment (string), for info purpose
- Planet (int), planet where the cluster is located
- Resource (int), not for default cluster

### Building
Represent an in-game building (house, harvester or factory).

- ID (int, PK)
- GameAccountID (int, FK), owner SWG Restoration III account
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
- HarvesterHopperSize (int), harvester capacity
- HarvesterResourceID (int, FK), ID of the resource the harvester is retrieving
- HarvesterResourceConcentration (TODO check name), float number from 0 to 1 (%tage)
- HarvesterAmount, contains the accumulated amount for all previous cycles. When the harvester is stopped, this field is updated with the added amount that has been harvested for this cycle. This field + the computation of the amount of the current cycle (if running) will give the total amount harvested to compute the hopper content size.
- HarvesterAmountLastUpdate (datetime?), last time we updated the HarvesterAmount (use to computed total amount)

## Site navigation, pages

### /
Home page.

### / Account
Industries account info and operations, subtree to be defined.

### / GameAccount
Page listing the accounts, their characters (up to four per account) with summary info for each character. With CRUD operations.

### / Crew
Manage a crew of Characters to share some features/info (e.g. Harvester Clusters, factories).

Crew members are declared (created, joined) at the `UserApplication` level (`user` from now on), a given user can be part of zero to one crew, a crew has a unique name and can be identified from its leader or its name.

A system of invitation allows users to request and join a crew. A user who is part of a crew select the Characters that are members.

A character that is member of a crew can "share" some of its lots for crew activities. A user can remove one character from the crew when it has no lots shared.

The leader can remove a user from the crew.

#### / Crew (`not a member`)
Possibility to create or join a crew.

##### / Crew / Create a crew (form)
Give a unique name, then create to become the leader of the crew.

##### / Crew / Request to join (form)
A user can make the request to join a crew by identifying it either from its name or from the user account's name.
Invitations are displayed in the crew leader's page and can be approved or rejected.

Once approved, the crew member has access to his `crew member` page.

#### / Crew (`crew leader`)

##### / Crew / Invitation request (component) 
Display pending requests to join, the leader can accept or reject them.

##### / Crew / Member overview
Display a table with all UserApplication accounts, their GameAccounts, their characters (only the ones that are members) and some overview info about the characters ()

#### / Crew (`crew member`)
Once granted, the member has access to the member page, an alert is displayed to inform him/her the invitation was granted. Closing the alert destroy the invitation object for good.

#### / Crew (`member or leader`)

##### / Crew / Characters (component)
Lists all the characters of all accounts in a table, with the possible actions:
- Join crew: can be made anytime.
- Leave crew: only if the character doesn't have lot assigned to the crew.

#### Invitation system
The invitation is made of a `from` and `to` users, a status and whether it is an invitation (`from` the leader `to` the future member) or a request to join (`from` the future member `to` the leader).

There can be only one invitation that involves a future member, the invitation object has to be destroyed before a request or invitation can be made again to that user. The reason behind is to ensure a user can't be part of more than one crew and to ease the whole implementation of the feature.

### / House
List houses, with CRUD.

### / Factory
List Factories, with CRUD.

### / Harvester
Main page

#### / Harvester / Inventory

List all the harvesters, which GameAccount owns each, with property summary. In this page we don't care which character actually owns it.
- CRUD operations
- Possibility to clone an existing harvester (type, BER, Capacity kept)

#### / Harvester / Operating

List all harvesters put down, grouped by their respective cluster. First cluster is the default one.
CRUD on clusters.
Add/remove harvester to a given cluster.
Operation on cluster: empty hopper, add maintenance, add power, destroy cluster
 


























