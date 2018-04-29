# Dependencies:

# index:
# - less-convert
# - plugins
# - extras
# - fixed-point

# less-convert:
# - selector-visitor

# plugins: none
# extras: none

# selector-visitor: none

# fixed-point: none



# Prefill globally-defined modules

MODULES =
  underscore: @_
  sizzle: @Sizzle
  less: @less
  eventemitter2: @EventEmitter2
  'selector-set': @SelectorSet


@_              = @__polyfills_originalGlobals['underscore']
@less           = @__polyfills_originalGlobals['less']
@EventEmitter2  = @__polyfills_originalGlobals['eventemitter2']
# @SelectorSet    = @__polyfills_originalGlobals['SelectorSet']

# This project is written to use RequireJS (and can be used this way for development) but
# Since all the files are concatenated, use a simple `define` function
# rather than RequireJS and all of its machinery.
#
# Also, the files are concatenated to match the dependency tree so the callback
# can be called immediately.
@define = (moduleName, deps, callback) ->
  args = for depName in deps
    # Split off the `cs!` when loading coffeescript files
    [first, second] = depName.split('!')
    depName = second or first
    throw new Error('Files are not concatenated based on their dependencies') unless MODULES[depName]
    MODULES[depName]
  val = callback.apply(@, args)
  MODULES[moduleName] = val
  val
