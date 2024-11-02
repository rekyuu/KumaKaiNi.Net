# KumaKaiNi.NET

## Commands

### General

- `!about`, `!help` - Get info about the current deployment and a link to this repo
- `!kuma`, `!ping` - Get a quick response from Kuma
- `!say <message>` - Responds with `<message>`
- `!projection` - Get a description of psychological projections
- `!moon` - Get the current moon phase

### RNG

- `!coin`, `!flip` - Flip a coin
- `!pick <choice1, choice2...>`, `!choose <choice1, choice2...>` - Pick from a list of choices
- `!roll [xdy]`, `!dice [xdy]` - Roll dice, optionally supply how many dice with sides using the format `xdy`, where x is the number of dice and y is the number of sides
- `!predict [message]` - Magic 8 ball style prediction

### Quotes

- `!quote` - Get a random quote
- `!quote (add|new) <message>` - **MOD ONLY**: Add a new quote
- `!quote (del|delete|rem|remove) <quote_id>` - **MOD ONLY**: Remove a quote

### Danbooru

[Danbooru Search Cheatsheet](https://danbooru.donmai.us/wiki_pages/help%3Acheatsheet)

- `!dan [tags]` - Get an image from Danbooru
- `!safe [tags]`, `!sfw [tags]` - Get an image from Danbooru that contains the tag `rating:safe` or `rating:sensitive`
- `!lewd [tags]` - Get an image from Danbooru that contains the tag `rating:questionable`
- `!xxx [tags]` - Get an image from Danbooru that contains the tag `rating:explicit`
- `!nsfw [tags]` - Get an image from Danbooru that contains either the tag `rating:questionable` or `rating:explicit`
- `!danalias (add|new|edit|mod|modify) <alias> <tag>` - Add a new alias for a Danbooru tag
- `!danalias (del|delete|rem|remove) <alias>` - Remove a Danbooru alias
- `!danban [tags]` - **ADMIN ONLY**: ban results that contain the supplied tags
- `!danunban [tags]` - **ADMIN ONLY**: unban results that contain the supplied tags

### Text Generation

- `!ai [prompt]`, `!gpt [prompt]` - Get an AI generated response
- `!gpt2` - Get a canned, pre-made AI generated response based on old chat history
- `!gdq` - Get a randomly generated GDQ message
- `!gptmodel [model]` - ADMIN ONLY: Get or set the OpenAI model
- `!gpttokenlimit [limit]` - ADMIN ONLY: Get or set the OpenAI token limit
- `!gptprompt` - **ADMIN ONLY**: Get the current system prompt
- `!gptrule [rule_id]` - **ADMIN ONLY**: Get the current system prompt rules or by ID
- `!gptrule (add|new) <rule>` - **ADMIN ONLY**: Add a new system prompt rule
- `!gptrule (update|modify) <rule_id> <rule>` - **ADMIN ONLY**: Update a system prompt rule
- `!gptrule (del|delete|rem|remove) <rule_id>` - **ADMIN ONLY**: Remove a system prompt rule

### Custom Commands

- `!command (add|new|edit|mod|modify) <command> <response>` - **MOD ONLY**: Add a new command
- `!command (del|delete|rem|remove) <command>` - **MOD ONLY**: Remove a command

## Infrastructure

Kuma works by having a centralized location that will accept requests from multiple clients. These clients currently are Telegram and Discord.

The RequestProcessor works by utilizing Redis streams. A client wills end a message to the Redis Stream, the processor will interpret the request and send a response if needed. This response goes into another Redis stream which the clients will interpret and relay it to where it needs to. The clients are responsible for interpreting this response with text, image embeds, etc.