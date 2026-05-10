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