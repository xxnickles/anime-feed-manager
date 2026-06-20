-- ValidatedField: client-side presentation for native form validation. htmx already
-- gates submission (it calls form.reportValidity() and refuses to send an invalid form),
-- which fires `invalid` on each failing field. We suppress the browser's native bubble and
-- paint the message into our own slot instead, toggling `data-error` to drive the daisyUI
-- error styling and the slide/fade reveal (see the data-error: Tailwind variant). On input
-- we clear the slot once the field is valid again, so an error shows only after a failed
-- attempt and never nags before the user has had a chance to fix it.
--
-- Lives in an external .hs file (not inline `_=`) so the <p[...]/> selectors don't trip
-- Razor's component-attribute parser. Opt a field in with `_="install ValidatedField"`,
-- or `_="install ValidatedField(blurValidate: true)"` to also validate when the field
-- loses focus (otherwise validation surfaces only on submit / explicit checks).
behavior ValidatedField(blurValidate)
  on invalid
    halt the event's default
    add @data-error to me
    set slot to the next <p[data-field-error]/>
    if slot
      set slot's textContent to my validationMessage
      add @data-error to slot
    end
  end

  on input
    set v to my validity
    if v's valid
      remove @data-error from me
      set slot to the next <p[data-field-error]/>
      if slot
        set slot's textContent to ''
        remove @data-error from slot
      end
    end
  end

  -- Opt-in: validate on focus loss. checkValidity() fires `invalid` (caught above) when
  -- the field is invalid, but unlike reportValidity() it shows no native bubble — so the
  -- error appears in our slot the moment the user tabs away, not only at submit time.
  on blur
    if blurValidate
      call my checkValidity()
    end
  end
end
