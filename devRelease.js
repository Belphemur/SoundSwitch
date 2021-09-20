'use strict'
import Upload from '@uploadcare/upload-client'

const UploadClient = Upload.default;
import * as fs from 'fs/promises';
import * as util from 'util';
import * as crypto from 'crypto';
import * as path from 'path';

import {exec} from 'child_process';

import {Webhook, MessageBuilder} from "discord-webhook-node";


const execAsync = util.promisify(exec);
const webhookUrl = process.argv[2];
const version = process.argv[3];
const repo = process.argv[4];
const filePath = process.argv[5];

const commitRegex = /^([\d\w]{7})\s(\w+)\((.+)\)(.+)/gm;
const commitSubstitution = `* **$2**(*$3*)$4 [$1](https://github.com/${repo}/commit/$1)`;
const uploadSecret = process.env.UPLOAD_SECRET;
const uploadPublic = process.env.UPLOAD_PUBLIC;


function generateSignature(secret, expire) {
    const hmac = crypto.createHmac('sha256', secret);
    hmac.update(expire);
    return hmac.digest('hex');
}

async function main() {


    const file = await fs.readFile(filePath);
    try {
        const client = new UploadClient({publicKey: uploadPublic});

        const expiry = (Math.round(Date.now() / 1000) + 120).toString();
        const signature = generateSignature(uploadSecret, expiry);

        const response = await client.uploadFile(file, {
            secureSignature: signature,
            secureExpire: expiry,
            fileName: path.basename(filePath)
        });

        const hook = new Webhook(webhookUrl);
        const embed = new MessageBuilder()
            .setAuthor("Beta Build", "https://soundswitch.aaflalo.me/img/beta.png")
            .setTitle(`New Build: ${version}`)
            .setURL(response.cdnUrl)
            .setColor("#0000FF");


        const result = await execAsync(`git log --no-merges --oneline -10`);

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

