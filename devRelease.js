'use strict'
const util = require('util');
const {Webhook, MessageBuilder} = require("discord-webhook-node");
const exec = util.promisify(require('child_process').exec);
const webhookUrl = process.argv[2];
const version = process.argv[3];
const repo = process.argv[4];
const commitRegex = /^([\d\w]{7})\s(\w+)\(([\w-]+)\)(.+)/gm;
const commitSubstitution = `* **$2**($3)$4 [$1](https://github.com/${repo}/commit/$1)`;

async function main() {
    const hook = new Webhook(webhookUrl);
    const embed = new MessageBuilder()
        .setAuthor("Beta Build", "https://soundswitch.aaflalo.me/img/beta.png")
        .setTitle(`New Build: ${version}`)
        .setURL("https://nc.aaflalo.me/s/8DKytYCTAbrn6yz")
        .setColor("#0000FF");

    try {
        let result = await exec("git describe --tags --abbrev=0");
        const describe = result.stdout.replace("\n", "");
        result = await exec(`git log  --no-merges --oneline ${describe}..HEAD`);

        const commits = result.stdout.replace(commitRegex, commitSubstitution);
        const description = `**Since ${describe}**\n\n${commits}`;
        embed.setDescription(description)
            .setTimestamp();
        console.log(description);
        hook.send(embed);
    } catch (e) {
        console.error(e);
    }
}

main()

