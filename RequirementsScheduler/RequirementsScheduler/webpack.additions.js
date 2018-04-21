// Shared rules[] we need to add
const sharedModuleRules = [
    // sass
    { test: /\.scss$/, loaders: ['to-string-loader', 'css-loader', 'sass-loader'] },
    // font-awesome
    { test: /\.(woff2?|ttf|eot|svg)$/, loader: 'url-loader?limit=10000' }
  ];
  
  
  module.exports = {
    sharedModuleRules
  };
  