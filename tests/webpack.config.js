const path = require("path");
const HtmlWebpackPlugin = require('html-webpack-plugin');

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

module.exports = (_env, options) => {

    var isDevelopment = options.mode === "development";

    return {
        // In development, bundle styles together with the code so they can also
        // trigger hot reloads. In production, put them in a separate CSS file.
        entry:
        {
            app: "./fableBuild/App.js"
        },
        // Add a hash to the output file name in production
        // to prevent browser caching if code changes
        output: {
            path: resolve("./temp"),
            filename: "app.js"
        },
        devtool: isDevelopment ? 'eval-source-map' : false,
        plugins:
            [
                // In production, we only need the bundle file
                isDevelopment && new HtmlWebpackPlugin({
                    filename: "./index.html",
                    template: "./index.html"
                })
            ].filter(Boolean),
        // Configuration for webpack-dev-server
        devServer: {
            port: 8080,
            hot: true
        }
    }
}
