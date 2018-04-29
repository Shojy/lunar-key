define 'polyfill-path/less-converters', [
  'underscore'
  'sizzle'
  'cs!polyfill-path/selector-visitor'
], (_, Sizzle, AbstractSelectorVisitor) ->

  PSEUDO_CLASSES = [
    'before'
    'after'
    'outside'
    'footnote-call'
    'footnote-marker'
    'running'
    'deferred' # From cnx-easybake
  ]

  # Convert CSS selectors to a valid CSS class name
  slugify = (str) ->
    str = str.replace(/\ +/g, ' ')                      # Collapse multiple whitespaces
    str = str.replace(/[^A-Za-z0-9>\[\]\(\)\.]/g, '-')  # Replace invalid chars with dash
    str = str.replace(/[\[\]\(\)]/g, '_')               # Replace brackets with underscore
    str = str.replace(/-+/g, '-')                       # Collapse multiple dashes
    str = str.replace(/>/g, '-child-')                  # Replace '>' with '-child-'
    str = str.replace(/\./g, '-dot-')                   # Replace '.' with '-dot-'
    str = str.replace(/-+/g, '-')                       # Collapse multiple '-' to one
    str = str.replace(/^-|-$/g, '')                     # Remove leading and trailing '-'
    return str

  freshClassIdCounter = 0
  freshClass = (selectorStr) ->
    selectorStr = slugify(selectorStr)
    return "js-polyfill-autoclass-#{freshClassIdCounter++}-#{selectorStr}"

  class AutogenClass
    # selector: less.tree.Selector # Used for calculating the priority (ie 'p > * > em')
    # rules: [less.tree.Rule]
    constructor: (@selectorStr, @elements, @rules) ->


  class PseudoExpander extends AbstractSelectorVisitor

    # Generates elements of the form `<span class="js-polyfill-pseudo-before"></span>`
    PSEUDO_ELEMENT_NAME: 'span' # 'polyfillpseudo'

    # Used to test if a selector is recognized by the browser by calling `node.querySelector(...)`
    selectorTestNode = document.createElement('span')

    constructor: (root, @set, @interestingSet, plugins) ->
      super(arguments...)

      @interestingRules = []
      for plugin in plugins
        for ruleName of plugin.rules or {}
          @interestingRules[ruleName] = true


    hasInterestingRules: (ruleSet) ->
      # Always return true if the meta-rule `*` is in the set
      return true if @interestingRules['*']
      for rule in ruleSet.rules
        return true if rule.name of @interestingRules


    operateOnElements: (frame, ruleSet, domSelector, pseudoSelector, originalSelector, selectorStr) ->
      if not pseudoSelector.elements.length
        # Simple selector; no pseudoSelectors

        # Test if the selector will work in a browser. If so, keep it. Otherwise, generate a new class for it.
        try
          selectorTestNode.querySelector(selectorStr)
          isBrowserSelector = true
        catch e
          isBrowserSelector = false

        if isBrowserSelector
          autoClass = new AutogenClass(selectorStr, originalSelector.elements, ruleSet.rules)
          # `@set` is what will be output as the result of CSS-polyfills so it should contain
          # the original selector.
          @set.add(selectorStr, autoClass)

        hasInterestingRules = @hasInterestingRules(ruleSet)

        if !isBrowserSelector or hasInterestingRules

          # The fixed-point iterator will use the autogenerated class name because it is =
          # quicker and easier to look up (esp if DOM nodes move)
          className = freshClass(selectorStr)
          for node in @getNodes(selectorStr)

            # node.classList can be empty
            node.className ?= ''

            # Skip SVG elements for now
            # node.className.baseVal
            if node.classList

              if !isBrowserSelector
                node.classList.add('js-polyfill-autoclass-keep')
              node.classList.add('js-polyfill-autoclass')
              node.classList.add('js-polyfill-interesting')
              node.classList.add(className)


          selectorStr = ".#{className}"
          autoClass = new AutogenClass(selectorStr, originalSelector.elements, ruleSet.rules)
          @interestingSet.add(selectorStr, autoClass)

          if !isBrowserSelector and !hasInterestingRules
            @set.add(selectorStr, autoClass)

      else

        newContexts = for originalContext in @getNodes(selectorStr)
          context = originalContext

          for pseudoNode in pseudoSelector.elements
            pseudoName = pseudoNode.value.replace('::', ':')

            simpleExpand = (op, pseudoName) =>
              # See if the pseudo element exists.
              # If not, add it to the DOM
              cls = "js-polyfill-pseudo-#{pseudoName}"

              pseudo = Sizzle(" > .#{cls}, > .js-polyfill-pseudo-outside > .#{cls}", context)[0]
              if pseudo
                context = pseudo
              else
                pseudo = document.createElement(@PSEUDO_ELEMENT_NAME)
                pseudo.classList.add('js-polyfill-pseudo')
                pseudo.classList.add(cls)
                switch op
                  when 'append' then context.appendChild(pseudo)
                  when 'prepend' then context.insertBefore(pseudo, context.firstChild)

                # Update the context to be current pseudo element
                context = pseudo


            selectorStr += pseudoName # Append to `selectorStr` so the autogen class contains the pseudo elements

            switch pseudoName
              when ':before'          then simpleExpand('prepend', 'before')
              when ':after'           then simpleExpand('append',  'after')
              when ':footnote-marker' then simpleExpand('prepend', 'footnote-marker')
              when ':footnote-call'   then simpleExpand('append',  'footnote-call')
              when ':running'         then simpleExpand('append',  'footnote-call')

              when ':outside'
                op          = 'wrap'
                pseudoName  = 'outside'
                # See if the pseudo element exists.
                # If not, add it to the DOM
                cls = "js-polyfill-pseudo-#{pseudoName}"
                if context.parentNode.classList.contains(cls)
                  context = context.parentNode
                else
                  # wrap the element
                  outside = document.createElement(@PSEUDO_ELEMENT_NAME)
                  outside.classList.add('js-polyfill-pseudo')
                  outside.classList.add(cls)

                  context.parentNode.replaceChild(outside, context)
                  outside.appendChild(context)
                  # Update the context to be current pseudo element
                  context = outside

              when ':deferred'
                # no-op
              else
                console.error("ERROR: Attempted to expand unsupported pseudo element #{pseudoName}")

          context # Add the (possibly) new context to the list

        # if contexts != originalContexts
        newClassName = freshClass(selectorStr)
        for context in newContexts
          context.classList.add('js-polyfill-autoclass')
          context.classList.add('js-polyfill-interesting')
          context.classList.add(newClassName)

        autoClass = new AutogenClass(selectorStr, pseudoSelector.elements, ruleSet.rules, @hasInterestingRules(ruleSet))
        # Do this **after** we create the AutogenClass so it squirrels away the _original_ selector for BASED_ON comment
        selectorStr = ".#{newClassName}"

        @set.add(selectorStr, autoClass)
        @interestingSet.add(selectorStr, autoClass) if @hasInterestingRules(ruleSet)


  return {
    PseudoExpander: PseudoExpander
  }
