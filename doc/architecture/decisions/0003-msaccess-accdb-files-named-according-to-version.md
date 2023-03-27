# 3. MSAccess accdb files named according to version

Date: 2023-03-27

## Status

Accepted

## Context

There's really no way to version-control MS Access artifacts.

## Decision

Instead of simply naming the MS Access database `freightr.accdb`, I'm going name it `freightr-vN.N.N.accdb`, using semantic versioning

## Consequences

 - Anyone working with the database will need to get the latest one (which should be obvious)
 - Additional storage
 - Much less likelihood of corrupting something or making an irreversible change
 - No specific process for pruning or retiring old versions, although all but the most recent are only kept for fallback purposes
