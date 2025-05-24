# TailwindCSS Integration Guide

This document explains the TailwindCSS integration setup for automatic CSS building during development and production builds.

## 🎯 **Overview**

The project is configured to automatically build TailwindCSS during:
- **Development builds**: Standard minified CSS
- **Production builds**: Minified + purged CSS for maximum optimization
- **Development watch**: Live reloading for development

## 📁 **File Structure**

```
TriangleSolver.Client/
├── tailwind.config.js          # TailwindCSS configuration
├── package.json                # NPM scripts for CSS building
├── tailwind_watch.bat          # Windows watch script
├── tailwind_watch.ps1          # PowerShell watch script
└── wwwroot/css/
    ├── app.input.css           # Source CSS file (edit this)
    └── app.output.css          # Generated CSS file (don't edit)
```

## ⚙️ **Configuration**

### `package.json` Scripts
- `css:dev` - Watch mode for development
- `css:build` - Single build with minification
- `css:prod` - Production build with minification + purging

## 🚀 **Usage**

### Development Workflow
```bash
# Start TailwindCSS in watch mode (auto-rebuilds on changes)
npm run css:dev
# OR use the convenience scripts:
./tailwind_watch.bat     # Windows
./tailwind_watch.ps1     # PowerShell
```

### Build Commands
```bash
# Development build (includes CSS build)
dotnet build

# Release build (includes optimized CSS)
dotnet build -c Release

# Production publish (maximum CSS optimization)
dotnet publish -c Release -p:PublishProfile=Production
```

## 🔧 **MSBuild Integration**

The project automatically runs TailwindCSS builds via MSBuild targets:

### `EnsureNodeModules`
- Runs `npm install` if `node_modules` doesn't exist
- Executes before CSS build targets

### `BuildTailwindCSS`
- Runs before every build
- Executes `npm run css:build` (minified CSS)

### `BuildTailwindCSSProduction`
- Runs before publish when Configuration=Release
- Executes `npm run css:prod` (minified + purged)

### `BuildProductionCSS` (Production Profile)
- Additional target for Production publish profile
- Ensures maximum optimization

## 📊 **Size Optimization**

The CSS optimization provides significant size reduction:

**Development CSS**: ~32KB (full TailwindCSS)
**Production CSS**: ~23KB (purged + minified)

### Purging Process
TailwindCSS automatically removes unused classes by scanning:
- All `.razor` component files
- All `.cs` code files
- All `.html` files

Only CSS classes actually used in your code are included in the final bundle.

## 🛠️ **Customization**

### Adding Custom CSS
Edit `wwwroot/css/app.input.css`:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Your custom CSS here */
.my-custom-class {
    /* Custom styles */
}
```

### Extending TailwindCSS
Modify `tailwind.config.js`:
```javascript
module.exports = {
  content: [/* ... */],
  theme: {
    extend: {
      colors: {
        'custom-blue': '#1e40af',
      },
    },
  },
  plugins: [
    // Add TailwindCSS plugins here
  ],
}
```

### Adding TailwindCSS Plugins
```bash
npm install @tailwindcss/forms @tailwindcss/typography
```

Then update `tailwind.config.js`:
```javascript
plugins: [
  require('@tailwindcss/forms'),
  require('@tailwindcss/typography'),
],
```

## 🔍 **Troubleshooting**

### CSS Not Updating
1. Check if `node_modules` exists: `npm install`
2. Manually build CSS: `npm run css:build`
3. Clear browser cache

### Build Errors
1. Ensure Node.js is installed and in PATH
2. Check TailwindCSS config syntax: `npx tailwindcss --help`
3. Verify input file exists: `wwwroot/css/app.input.css`

### Missing Styles in Production
1. Check if classes are being purged incorrectly
2. Add classes to safelist in `tailwind.config.js`:
```javascript
module.exports = {
  content: [/* ... */],
  safelist: [
    'bg-red-500',
    'text-center',
    // Classes that might be missed by purging
  ],
}
```

## 🎨 **Best Practices**

1. **Always edit `app.input.css`**, never `app.output.css`
2. **Use watch mode during development** for live updates
3. **Test production builds** to ensure no styles are accidentally purged
4. **Keep TailwindCSS classes in HTML/Razor** - avoid building class names dynamically
5. **Use the safelist** for dynamically generated classes

## 🔄 **Workflow Summary**

1. **Development**: Run `npm run css:dev` in terminal for live CSS updates
2. **Building**: CSS automatically builds during `dotnet build`
3. **Publishing**: Optimized CSS automatically builds during `dotnet publish`
4. **Production**: Use `dotnet publish -c Release -p:PublishProfile=Production` for maximum optimization

The integration ensures your CSS is always optimized for the target environment! 🎉 