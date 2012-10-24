import os, re
try:
    import simplejson as json
except:
    import json

# http://jsonlint.com/
def parse(path):
    '''
    if file contains a valid JSON list, except "",
    parse(filepath) returns an ordered list of keyword argument
    dicts using the first line in input list as keyword names,
    and the following as keyword values.

    everything to the right of the comment delim iter '%' is ignored

    the order of the columns is not significant

    [
        [description,         method,      uri,              data,          username, password,		expected statuscode,  	reason],
        %-----------------------------------------------------------------------------------------------------------------------------------------------
        [get gamefactory,     GET,         /lobby/mygame/,   "",            "", "",                 404,                  	Not found],
	[get gamefactory,     POST,         /lobby/mygame/,   "",            "", "",                 404,                  	Not found]
    ]

    '''
   
    decoder = json.JSONDecoder(parse_int=True, strict=True)

    with open(path, 'rb') as f:
        raws = f.readlines()

    server, raws = raws[0].strip(), raws[1:]
    fraw = _prune_comments(raws)
    fraw = _prune_sp_tokens(fraw)
    valid_json = _commentify(fraw)

    try:
        json_list = decoder.decode(valid_json)
        kwargs = _extract_kwargs(json_list)
    except Exception as e:
        print 'uuuups, not valid json!'
        print valid_json
        raise
    return server, kwargs


def _extract_kwargs(json_list):
    kwarg_names = json_list[0]

    res = []
    for test in json_list[1:]:
        kwargs = {}
        for i, value in enumerate(test):
            if not value: None
            kwargs[kwarg_names[i].replace(' ', '_')] = value

        res.append(kwargs)
    return res

def _prune_comments(raws):
    raw = ''
    for line in raws:
        match = line.find('%')
        if match != -1:
            raw += line[:match]
        else:
            raw += line

    return raw


def _prune_sp_tokens(fraw):
    return re.sub('[\t\s]+',' ',fraw)


def _commentify(raw):
    '''
    inputs the " for the raw to be valid JSON,
    '''

    escaping = False
    search_wstart = True
    search_wend = False
    last_char = None
        
    formatted_raw = ''
    for char in raw:
        new_last_char = char
        
        if not char or char == ' ':
            pass
        
        elif char == '"':
            escaping = not escaping
            pass
            

        elif char != '"' and escaping:
            pass

        elif char == '[':
            search_wend = False
            search_wstart = True

        elif char == ']':
            if search_wend:
                char = '"' + char
            elif search_wstart:
                if last_char == ']':
                    pass
                else:
                    char = '""' + char

            search_wend = False
            search_wstart = True
            
        elif char == ',' :
            if last_char == ']':
                pass
            elif last_char == '"':
                pass
            elif search_wend:
                char = '"' + char
            elif search_wstart:
                char = '""' + char
            
            search_wend = False
            search_wstart = True

        elif search_wstart:
            char = '"' + char
              
            search_wstart = False
            search_wend = True


        formatted_raw += char
        if char != ' ':
            last_char = new_last_char
            
    return formatted_raw

if __name__ == '__main__':
    print parse('example.test')
    

# TODO:
# read keyword names
# use these coupled with each column, as input to testfunction