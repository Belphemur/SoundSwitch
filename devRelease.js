'use strict'
import {writeFile} from 'fs/promises';
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


        const nightlyPrefix = "nightly/";
        const list = (await bucket.list(nightlyPrefix)).sort((a, b) => a.timestamp > b.timestamp ? 1 : -1);

        if(list.length >= 9) {
            await bucket.removeFile(list[0]);
        }

        const zipResponse = await bucket.upload(filePath, `${nightlyPrefix}${fileBasename}`);

        const versionData = {latest: version, published: new Date(), url: zipResponse.url};
        await writeFile("version.json", JSON.stringify(versionData));
        const currentVersionResponse = await bucket.upload("version.json", `${nightlyPrefix}version.json`);


        const hook = new Webhook(webhookUrl);
        const embed = new MessageBuilder()
            .setAuthor("Beta Build", "https://soundswitch.aaflalo.me/img/beta.png")
            .setTitle(`New Build: ${version}`)
            .setURL(zipResponse.url)
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

