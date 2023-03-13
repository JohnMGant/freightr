# freightr Usage Guide

The **freightr** data model centers on three primary entities; the customer, the carrier, and the order.
An order represents a contract for the carrier to transport goods from a shipper's location to a consignee's location, with possible stops along the way.
Shippers and consignees are both types of customer.
The third type of customer is a payer, which is the organization that is funding the transportation.

## Customers

Each customer has a unique identifier, which is generally a sequential number.
Customers that are eligible to be a payer for an order are identified by the "Is Payer" flag on the customer record.
Each customer also has one or more contacts.
One contact for each customer can be identited as the primary contact.

A customer can have multiple locations.
Each location can be designated as being eligible for pickups (freight can be shipped from there), dropoffs (freight can be delivered there), or both.
Each location can also have a specific contact assigned from the customer's list of contacts.

## Carriers

A carrier is a company that transports freight from one location to another.
This could be, for example, a trucking, rail, or air freight company. But it could also be a broker or intermediary.
Each carrier is capable of one or more load types.
For a trucking company, these load types might be dry van, refrigerated, flatbed, hazmat, LTL, etc. This list is customizable to support the particular types of freight a user may deal with.

## Orders

An order consists of two or more stops.
Each stop can consist of zero or more cargo changes.
In a typical "Point A to Point B" order, there would be two stops, each of which would have one cargo change.
The first stop would have cargo change of type "Trailer Load" or "Trailer Hook."
The second stop would have a cargo change of type "Trailer Unload" or "Trailer Drop."
More complex orders might have multiple stops, each of which could have load/unload or drop/hook changes to the cargo.
It's also possible for a stop (other than the first and last) to have no cargo changes, such as a rest stop or a delay at a freight yard.

The information associated with an order stop includes the distance from the last stop and the charge per distance unit.
Since **freightr** is intended to be international in scope, it doesn't specific the distance units or other units of measurement.

In addition to the per-mile charges, users can create additional charges at the order level.
For example, if there's a fuel surcharge or some kind of documentation charge, these can be created here.

## Various user types

**freightr** is designed to be flexible enough to support different types of users.
Listed below are the types of entities that would likely be created for various user types:
|User type|Customers|Carriers|Orders|
| - | - | - | - |
|Small manufacturer|A customer for themselves, with locations for their plants if they have more than one; customers and locations for all the places they ship things to | Any carriers or brokers they use | Orders they ship |
|Chain of stores|A customer for themselves, with locations of their stores; customers and locations for their distributors | Any carriers or brokers they or their shippers use | Orders they're receiving |
|Freight broker|"Payer" customers for all of their direct customers, customers and locations for their shippers and consignees|Any carriers they use|Orders their filling|
