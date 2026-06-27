# Ideas for leaderboard query language
- Similar to Django order_by?
- Have a list of attributes fo elements:
    - 
- json? simpler query language?
- query -> json?

- Simple message query: `cnt:msg`
- Count emojis?: `cnt:msg.emoji`
- Specific emoji: `cnt:msg.emoji(:thumbs_up:)`
- Cooldown: `cooldown:60s;cnt:msg`
- Message content: `cnt:msg.contains('word')`

# Design for query language

## Goal

Human readable way for Discord users/admins to configure custom leaderboards to be tracked on a server

## Idea

The query essentially contains the filter, score and cooldown associated for it with a separator. Potentially make it editable visually later as well

## Broad syntax

```
filter:<condition> | xp:<expression> | cooldown:<time>
```

Only filter will be required, xp will default to the server default

## Filters

Can be Anded with comma. Inside filter comma does OR

| Condition | Meaning |
|-----------|---------|
| `msg` | Any message |
| `msg(word)` | Message containing that word |
| `re(👍)` | Message got a reaction like this, also takes emoji ID |
| `channel(ID)` | Message was in a specific channel |
| `role(ID)` | Author had specific role |
| `user(ID)` | Author has specific role |
| `name(str)` | Nickname contains string |
| `len(n)` | length is bigger than n |
| `len` | Message has attachment |
| `img` | Message has image attachment |
| `link` | Message has link |
| `time(0900-1700)` | Message was during UTC timeframe. Time in 24h |
| `day(mon,wed)` | Message was during day in UTC. Days as three letter abreviation or index |
| `men` | Message contains mention |
| `men(role_id)` | Message contains mention to specific role |
| `men(user_id)` | Message contains mention to specific user |

Filters are negated with a !

## Score is flat score for now

```
score:n
```