import yaml
import transpiler
import json

if __name__ == "__main__":
    with open("cards.yaml", encoding='utf-8') as f:
        try:
            data = yaml.safe_load(f)
            cards = transpiler.Transpiler(data).transpile()
            print (json.dumps([c.to_dict() for c  in cards], indent=4))
        except yaml.YAMLError as exc:
            print(exc)
        