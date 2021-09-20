'use strict'
import * as util from 'util';
import * as path from 'path';
import {exec} from 'child_process';
import {Webhook, MessageBuilder} from "discord-webhook-node";
import Bucket from "backblaze";

const execAsync = util.promisify(exec);
const webhookUrl = process.argv[2];
const version = process.argv[3];
const repo = process.argv[4];
const filePath = process.argv[5];

const commitRegex = /^([\d\w]{7})\s(\w+)\((.+)\)(.+)/gm;
const commitSubstitution = `* **$2**(*$3*)$4 [$1](https://github.com/${repo}/commit/$1)`;
const uploadSecret = process.env.UPLOAD_SECRET;
const uploadPublic = process.env.UPLOAD_PUBLIC;
const bucketName = process.env.UPLOAD_BUCKET;

async function main() {

    try {

        const bucket = Bucket(bucketName, {
            id: uploadPublic,
            key: uploadSecret
        });

        const fileBasename = path.basename(filePath);

        const response = await bucket.upload(filePath, `nightly/${fileBasename}`);
        const hook = new Webhook(webhookUrl);
        const embed = new MessageBuilder()
            .setAuthor("Beta Build", "https://soundswitch.aaflalo.me/img/beta.png")
            .setTitle(`New Build: ${version}`)
            .setURL(response.url)
            .setColor("#0000FF");


        const result = await execAsync('git log --no-merges --oneline -10');

        const commits = result.stdout.replace(commitRegex, commitSubstitution);
        const description = `**Last 10 commits**\n\n${commits}`;
        embed.setDescription(description)
            .setTimestamp();
        console.log(description);
        await hook.send(embed);
    } catch (e) {
        console.error(e);
        process.exit(1);
    }
}

main()

