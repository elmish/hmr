import { resolve, basename } from 'path'
import chalk from 'chalk'
import shelljs from 'shelljs'

const log = console.log

import { release } from "./release-core.js"

const getEnvVariable = function (varName) {
    const value = process.env[varName];
    if (value === undefined) {
        log(error(`Missing environnement variable ${varName}`))
        process.exit(1)
    } else {
        return value;
    }
}

// Check that we have enought arguments
if (process.argv.length < 4) {
    log(chalk.red("Missing arguments"))
    process.exit(1)
}

const cwd = process.cwd()
const baseDirectory = resolve(cwd, process.argv[2])
const projectFileName = process.argv[3]

const NUGET_KEY = getEnvVariable("NUGET_KEY")

release({
    baseDirectory: baseDirectory,
    projectFileName: projectFileName,
    versionRegex: /(^\s*<Version>)(.*)(<\/Version>\s*$)/gmi,
    publishFn: async (versionInfo) => {

        const packResult =
            shelljs.exec(
                "dotnet pack -c Release",
                {
                    cwd: baseDirectory
                }
            )

        if (packResult.code !== 0) {
            throw "Dotnet pack failed"
        }

        const fileName = basename(projectFileName, ".fsproj")

        const pushNugetResult =
            shelljs.exec(
                `dotnet nuget push bin/Release/${fileName}.${versionInfo.version}.nupkg -s nuget.org -k ${NUGET_KEY}`,
                {
                    cwd: baseDirectory
                }
            )

        if (pushNugetResult.code !== 0) {
            throw "Dotnet push failed"
        }
    }
})
